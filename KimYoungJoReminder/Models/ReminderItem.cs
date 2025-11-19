using System;

namespace KimYoungJoReminder.Models
{
    /// <summary>
    /// 개별 리마인더 아이템
    /// </summary>
    public class ReminderItem
    {
        /// <summary>
        /// 리마인더가 시작될 시간 (초 단위)
        /// </summary>
        public int TimeInSeconds { get; set; }

        /// <summary>
        /// 리마인더 지속 시간 (초 단위)
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// 표시할 텍스트
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 이미 알림을 표시했는지 여부 (재생 중 중복 방지)
        /// </summary>
        public bool HasTriggered { get; set; }

        /// <summary>
        /// 타임라인에서 배치될 레인 (행) 번호 (0부터 시작)
        /// </summary>
        public int Lane { get; set; }

        public ReminderItem(int timeInSeconds, string text, int duration = 1)
        {
            TimeInSeconds = timeInSeconds;
            Text = text;
            Duration = duration;
            HasTriggered = false;
            Lane = 0;
        }

        /// <summary>
        /// 종료 시간 계산
        /// </summary>
        public int GetEndTimeInSeconds()
        {
            return TimeInSeconds + Duration;
        }

        /// <summary>
        /// 시작 시간을 MM:SS 형식으로 반환
        /// </summary>
        public string GetFormattedTime()
        {
            int minutes = TimeInSeconds / 60;
            int seconds = TimeInSeconds % 60;
            return string.Format("{0:D2}:{1:D2}", minutes, seconds);
        }

        /// <summary>
        /// 종료 시간을 MM:SS 형식으로 반환
        /// </summary>
        public string GetFormattedEndTime()
        {
            int endTime = GetEndTimeInSeconds();
            int minutes = endTime / 60;
            int seconds = endTime % 60;
            return string.Format("{0:D2}:{1:D2}", minutes, seconds);
        }
    }
}
