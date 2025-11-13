using System;
using System.Collections.Generic;
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
        /// 1초마다 실행되는 타이머
        /// </summary>
        private DispatcherTimer _timer;

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

            // 1초마다 실행되는 타이머 설정
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// 타이머 틱 이벤트 (1초마다 실행)
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentTimeInSeconds++;

            // 시간 업데이트 이벤트 발생
            TimeUpdated?.Invoke(this, CurrentTimeInSeconds);

            // 해당 시간의 리마인더 체크
            CheckAndTriggerReminders();

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

            _timer.Stop();
            ChangeState(TimelineState.Paused);
        }

        /// <summary>
        /// 정지 (타이머만 멈춤, 시간은 유지)
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
        }

        /// <summary>
        /// 리셋 (처음부터 다시)
        /// </summary>
        public void Reset()
        {
            Stop();
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
        public bool AddReminder(int timeInSeconds, string text)
        {
            // 유효성 검사
            if (timeInSeconds < 0 || timeInSeconds >= MAX_TIMELINE_SECONDS)
                return false;

            // 동일 시간대에 이미 리마인더가 있는지 체크 (초당 1개만 허용)
            if (_reminders.Any(r => r.TimeInSeconds == timeInSeconds))
                return false;

            _reminders.Add(new ReminderItem(timeInSeconds, text));
            return true;
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
    }
}
