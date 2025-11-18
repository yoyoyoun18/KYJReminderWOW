using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace KimYoungJoReminder.Pages
{
    /// <summary>
    /// 토스트 알림 윈도우
    /// </summary>
    public partial class ToastNotification : Window
    {
        private DispatcherTimer _autoCloseTimer;

        public ToastNotification(string title, string message, int durationSeconds = 5)
        {
            InitializeComponent();

            txtTitle.Text = title;
            txtMessage.Text = message;

            // 화면 우측 하단에 위치 설정
            PositionToastAtBottomRight();

            // 자동으로 닫히는 타이머 설정
            _autoCloseTimer = new DispatcherTimer();
            _autoCloseTimer.Interval = TimeSpan.FromSeconds(durationSeconds);
            _autoCloseTimer.Tick += (s, e) =>
            {
                _autoCloseTimer.Stop();
                CloseWithAnimation();
            };
            _autoCloseTimer.Start();

            // 페이드 인 애니메이션
            Loaded += (s, e) => FadeIn();

            // 클릭하면 닫기
            MouseLeftButtonDown += (s, e) => CloseWithAnimation();
        }

        /// <summary>
        /// 화면 우측 하단에 토스트 위치 설정
        /// </summary>
        private void PositionToastAtBottomRight()
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 20;
            Top = workArea.Bottom - Height - 20;
        }

        /// <summary>
        /// 페이드 인 애니메이션
        /// </summary>
        private void FadeIn()
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            BeginAnimation(OpacityProperty, fadeIn);
        }

        /// <summary>
        /// 페이드 아웃 후 닫기
        /// </summary>
        private void CloseWithAnimation()
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            fadeOut.Completed += (s, e) => Close();
            BeginAnimation(OpacityProperty, fadeOut);
        }

        /// <summary>
        /// 토스트 알림 표시 (정적 메서드)
        /// </summary>
        public static void Show(string title, string message, int durationSeconds = 5)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var toast = new ToastNotification(title, message, durationSeconds);
                toast.Show();
            });
        }
    }
}
