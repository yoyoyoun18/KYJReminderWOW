using System.Collections.Generic;

namespace KimYoungJoReminder.Models
{
    /// <summary>
    /// Boss Profile 데이터 (JSON 직렬화용)
    /// </summary>
    public class BossProfile
    {
        /// <summary>
        /// 프로필 이름 (Boss 1, Boss 2, etc.)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 리마인더 목록
        /// </summary>
        public List<ReminderItemData> Reminders { get; set; }

        public BossProfile()
        {
            Reminders = new List<ReminderItemData>();
        }
    }

    /// <summary>
    /// JSON 직렬화용 ReminderItem 데이터
    /// (HasTriggered 제외 - 실행 시점마다 초기화)
    /// </summary>
    public class ReminderItemData
    {
        public int TimeInSeconds { get; set; }
        public int Duration { get; set; }
        public string Text { get; set; }

        // Lane은 저장하지 않음 - 로드 시 자동 할당
    }
}
