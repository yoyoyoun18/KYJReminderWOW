using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;

namespace KimYoungJoReminder.Models
{
    /// <summary>
    /// 타임라인 재생 및 리마인더 관리
    /// </summary>
    public class TimelineManager
    {
        /// <summary>
        /// 타임라인 최대 길이 (초 단위) - 현재 3분 고정
        /// </summary>
        public const int MAX_TIMELINE_SECONDS = 180;

        /// <summary>
        /// 현재 재생 시간 (초 단위)
        /// </summary>
        public int CurrentTimeInSeconds { get; private set; }

        /// <summary>
        /// 현재 타임라인 상태
        /// </summary>
        public TimelineState State { get; private set; }

        /// <summary>
        /// 등록된 리마인더 목록
        /// </summary>
        private List<ReminderItem> _reminders;

        /// <summary>
        /// UI 업데이트용 타이머 (0.1초마다 실행)
        /// </summary>
        private DispatcherTimer _timer;

        /// <summary>
        /// 실제 경과 시간 측정용 Stopwatch
        /// </summary>
        private Stopwatch _stopwatch;

        /// <summary>
        /// Pause 시점까지의 누적 시간 (초)
        /// </summary>
        private int _pausedTimeInSeconds;

        /// <summary>
        /// 시간이 업데이트될 때 발생하는 이벤트 (UI 업데이트용)
        /// </summary>
        public event EventHandler<int> TimeUpdated;

        /// <summary>
        /// 리마인더가 트리거될 때 발생하는 이벤트
        /// </summary>
        public event EventHandler<ReminderItem> ReminderTriggered;

        /// <summary>
        /// 상태가 변경될 때 발생하는 이벤트
        /// </summary>
        public event EventHandler<TimelineState> StateChanged;

        public TimelineManager()
        {
            _reminders = new List<ReminderItem>();
            CurrentTimeInSeconds = 0;
            State = TimelineState.Stopped;
            _pausedTimeInSeconds = 0;

            // UI 업데이트용 타이머 (0.1초마다 실행하여 부드러운 업데이트)
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;

            // Stopwatch 초기화
            _stopwatch = new Stopwatch();
        }

        /// <summary>
        /// 타이머 틱 이벤트 (0.1초마다 실행)
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Stopwatch로부터 실제 경과 시간 계산 (초 단위)
            int newTimeInSeconds = _pausedTimeInSeconds + (int)_stopwatch.Elapsed.TotalSeconds;

            // UI는 매 틱마다 업데이트 (소수점 표시를 위해)
            TimeUpdated?.Invoke(this, newTimeInSeconds);

            // 정수 초가 바뀔 때만 리마인더 체크
            if (newTimeInSeconds != CurrentTimeInSeconds)
            {
                CurrentTimeInSeconds = newTimeInSeconds;
                CheckAndTriggerReminders();
            }

            // 최대 시간 도달 체크
            if (CurrentTimeInSeconds >= MAX_TIMELINE_SECONDS)
            {
                Stop();
                ChangeState(TimelineState.Completed);
            }
        }

        /// <summary>
        /// 현재 시간에 해당하는 리마인더가 있는지 체크하고 트리거
        /// </summary>
        private void CheckAndTriggerReminders()
        {
            var reminderToTrigger = _reminders.FirstOrDefault(r =>
                r.TimeInSeconds == CurrentTimeInSeconds && !r.HasTriggered);

            if (reminderToTrigger != null)
            {
                reminderToTrigger.HasTriggered = true;
                ReminderTriggered?.Invoke(this, reminderToTrigger);
            }
        }

        /// <summary>
        /// 재생 시작 또는 재개
        /// </summary>
        public void Play()
        {
            if (State == TimelineState.Completed)
                return;

            // Stopwatch 시작 또는 재개
            _stopwatch.Start();
            _timer.Start();
            ChangeState(TimelineState.Playing);
        }

        /// <summary>
        /// 일시정지
        /// </summary>
        public void Pause()
        {
            if (State != TimelineState.Playing)
                return;

            // Stopwatch 정지 및 누적 시간 저장
            _stopwatch.Stop();
            _pausedTimeInSeconds = CurrentTimeInSeconds;

            _timer.Stop();
            ChangeState(TimelineState.Paused);
        }

        /// <summary>
        /// 정지 (타이머만 멈춤, 시간은 유지)
        /// </summary>
        public void Stop()
        {
            _stopwatch.Stop();
            _timer.Stop();
        }

        /// <summary>
        /// 리셋 (처음부터 다시)
        /// </summary>
        public void Reset()
        {
            Stop();

            // Stopwatch 완전 초기화
            _stopwatch.Reset();
            _pausedTimeInSeconds = 0;
            CurrentTimeInSeconds = 0;

            // 모든 리마인더의 트리거 상태 초기화
            foreach (var reminder in _reminders)
            {
                reminder.HasTriggered = false;
            }

            ChangeState(TimelineState.Stopped);
            TimeUpdated?.Invoke(this, CurrentTimeInSeconds);
        }

        /// <summary>
        /// 리마인더 추가
        /// </summary>
        public bool AddReminder(int timeInSeconds, string text, int duration = 1)
        {
            // 유효성 검사
            if (timeInSeconds < 0 || timeInSeconds >= MAX_TIMELINE_SECONDS)
                return false;

            // 종료 시간이 타임라인을 초과하지 않도록
            if (timeInSeconds + duration > MAX_TIMELINE_SECONDS)
                duration = MAX_TIMELINE_SECONDS - timeInSeconds;

            // 동일 시간대에 이미 리마인더가 있는지 체크
            if (_reminders.Any(r => r.TimeInSeconds == timeInSeconds))
                return false;

            // 새 리마인더 생성
            var newReminder = new ReminderItem(timeInSeconds, text, duration);

            // Lane 자동 할당
            newReminder.Lane = FindAvailableLane(newReminder);

            _reminders.Add(newReminder);
            return true;
        }

        /// <summary>
        /// 리마인더가 배치될 수 있는 가장 낮은 Lane 찾기
        /// </summary>
        private int FindAvailableLane(ReminderItem newReminder)
        {
            int lane = 0;

            while (true)
            {
                // 현재 Lane에 있는 리마인더들 가져오기
                var remindersInLane = _reminders.Where(r => r.Lane == lane);

                // 이 Lane에서 겹치는 리마인더가 있는지 확인
                bool hasOverlap = remindersInLane.Any(r => IsOverlapping(r, newReminder));

                if (!hasOverlap)
                {
                    // 겹치지 않으면 이 Lane 사용
                    return lane;
                }

                // 겹치면 다음 Lane 확인
                lane++;

                // 무한 루프 방지 (최대 100개 Lane)
                if (lane >= 100)
                    return 0;
            }
        }

        /// <summary>
        /// 두 리마인더가 시간상 겹치는지 확인
        /// </summary>
        private bool IsOverlapping(ReminderItem a, ReminderItem b)
        {
            // A의 시작이 B의 끝보다 작고, B의 시작이 A의 끝보다 작으면 겹침
            return a.TimeInSeconds < b.GetEndTimeInSeconds() &&
                   b.TimeInSeconds < a.GetEndTimeInSeconds();
        }

        /// <summary>
        /// 현재 사용 중인 최대 Lane 번호 반환
        /// </summary>
        public int GetMaxLane()
        {
            if (_reminders.Count == 0)
                return 0;

            return _reminders.Max(r => r.Lane);
        }

        /// <summary>
        /// 특정 시간의 리마인더 삭제
        /// </summary>
        public bool RemoveReminder(int timeInSeconds)
        {
            var reminder = _reminders.FirstOrDefault(r => r.TimeInSeconds == timeInSeconds);
            if (reminder != null)
            {
                _reminders.Remove(reminder);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 모든 리마인더 가져오기
        /// </summary>
        public List<ReminderItem> GetAllReminders()
        {
            return new List<ReminderItem>(_reminders);
        }

        /// <summary>
        /// 상태 변경 및 이벤트 발생
        /// </summary>
        private void ChangeState(TimelineState newState)
        {
            if (State != newState)
            {
                State = newState;
                StateChanged?.Invoke(this, newState);
            }
        }

        /// <summary>
        /// 현재 시간을 MM:SS 형식으로 반환
        /// </summary>
        public string GetFormattedCurrentTime()
        {
            int minutes = CurrentTimeInSeconds / 60;
            int seconds = CurrentTimeInSeconds % 60;
            return string.Format("{0:D2}:{1:D2}", minutes, seconds);
        }

        /// <summary>
        /// 현재 시간을 소수점 포함하여 반환 (MM:SS.F 형식)
        /// </summary>
        public string GetFormattedCurrentTimeWithDecimal()
        {
            double totalSeconds = _pausedTimeInSeconds + _stopwatch.Elapsed.TotalSeconds;
            int minutes = (int)totalSeconds / 60;
            double seconds = totalSeconds % 60;
            return string.Format("{0:D2}:{1:00.0}", minutes, seconds);
        }

        /// <summary>
        /// 현재 시간을 소수점 포함한 초 단위로 반환
        /// </summary>
        public double GetCurrentTimeInSecondsWithDecimal()
        {
            return _pausedTimeInSeconds + _stopwatch.Elapsed.TotalSeconds;
        }
    }
}
