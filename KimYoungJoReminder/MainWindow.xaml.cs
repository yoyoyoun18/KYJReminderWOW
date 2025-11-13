using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using KimYoungJoReminder.Models;
using KimYoungJoReminder.Views;

namespace KimYoungJoReminder
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 1초당 픽셀 수 (타임라인 스케일)
        /// </summary>
        private const int PIXELS_PER_SECOND = 10;

        /// <summary>
        /// 타임라인 관리자
        /// </summary>
        private TimelineManager _timelineManager;

        /// <summary>
        /// 현재 재생 위치를 나타내는 라인
        /// </summary>
        private Line _playheadLine;

        /// <summary>
        /// 리마인더 마커 딕셔너리 (시간 -> Rectangle)
        /// </summary>
        private Dictionary<int, Rectangle> _reminderMarkers;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        private void Initialize()
        {
            _timelineManager = new TimelineManager();
            _reminderMarkers = new Dictionary<int, Rectangle>();

            // TimelineManager 이벤트 구독
            _timelineManager.TimeUpdated += OnTimeUpdated;
            _timelineManager.ReminderTriggered += OnReminderTriggered;
            _timelineManager.StateChanged += OnStateChanged;

            // 타임라인 초기화
            DrawTimelineGrid();
            CreatePlayhead();
        }

        /// <summary>
        /// 타임라인 눈금 그리기
        /// </summary>
        private void DrawTimelineGrid()
        {
            // 10초마다 세로선 및 시간 텍스트 표시
            for (int second = 0; second <= TimelineManager.MAX_TIMELINE_SECONDS; second += 10)
            {
                double x = second * PIXELS_PER_SECOND;

                // 세로선
                Line gridLine = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = timelineCanvas.Height,
                    Stroke = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                    StrokeThickness = 1
                };
                timelineCanvas.Children.Add(gridLine);

                // 시간 텍스트
                int minutes = second / 60;
                int seconds = second % 60;
                string timeText = string.Format("{0:D2}:{1:D2}", minutes, seconds);

                TextBlock timeLabel = new TextBlock
                {
                    Text = timeText,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    FontSize = 10
                };
                Canvas.SetLeft(timeLabel, x + 2);
                Canvas.SetTop(timeLabel, 5);
                timelineCanvas.Children.Add(timeLabel);
            }
        }

        /// <summary>
        /// 재생 헤드(현재 위치 표시선) 생성
        /// </summary>
        private void CreatePlayhead()
        {
            _playheadLine = new Line
            {
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = timelineCanvas.Height,
                Stroke = new SolidColorBrush(Color.FromRgb(78, 201, 176)), // 청록색
                StrokeThickness = 2
            };
            timelineCanvas.Children.Add(_playheadLine);
        }

        /// <summary>
        /// 재생 헤드 위치 업데이트
        /// </summary>
        private void UpdatePlayheadPosition(int timeInSeconds)
        {
            double x = timeInSeconds * PIXELS_PER_SECOND;
            _playheadLine.X1 = x;
            _playheadLine.X2 = x;
        }

        /// <summary>
        /// 타임라인 Canvas 더블클릭 이벤트 (현재는 단일 클릭으로 처리)
        /// </summary>
        private void TimelineCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 재생 중에는 클릭 방지
            if (_timelineManager.State == TimelineState.Playing)
                return;

            // 더블클릭 체크
            if (e.ClickCount == 2)
            {
                Point clickPosition = e.GetPosition(timelineCanvas);
                int clickedSecond = (int)(clickPosition.X / PIXELS_PER_SECOND);

                // 범위 체크
                if (clickedSecond < 0 || clickedSecond >= TimelineManager.MAX_TIMELINE_SECONDS)
                    return;

                // 리마인더 입력 창 표시
                ShowReminderInputDialog(clickedSecond);
            }
        }

        /// <summary>
        /// 리마인더 입력 다이얼로그 표시
        /// </summary>
        private void ShowReminderInputDialog(int timeInSeconds)
        {
            // 간단한 InputBox 구현
            var inputWindow = new Window
            {
                Title = "Add Reminder",
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 45))
            };

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 시간 표시
            var timeLabel = new TextBlock
            {
                Text = $"Time: {FormatTime(timeInSeconds)}",
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(timeLabel, 0);
            grid.Children.Add(timeLabel);

            // 텍스트 입력
            var textBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(5)
            };
            Grid.SetRow(textBox, 1);
            grid.Children.Add(textBox);

            // 버튼 패널
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0)
            };
            okButton.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (_timelineManager.AddReminder(timeInSeconds, textBox.Text))
                    {
                        AddReminderMarker(timeInSeconds, textBox.Text);
                        inputWindow.DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show("A reminder already exists at this time.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80,
                Height = 30
            };
            cancelButton.Click += (s, e) => { inputWindow.DialogResult = false; };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            inputWindow.Content = grid;
            inputWindow.ShowDialog();
        }

        /// <summary>
        /// 타임라인에 리마인더 마커 추가
        /// </summary>
        private void AddReminderMarker(int timeInSeconds, string text)
        {
            double x = timeInSeconds * PIXELS_PER_SECOND;

            // 마커 사각형
            Rectangle marker = new Rectangle
            {
                Width = 8,
                Height = timelineCanvas.Height - 30,
                Fill = new SolidColorBrush(Color.FromRgb(255, 165, 0)), // 주황색
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1
            };

            Canvas.SetLeft(marker, x - 4);
            Canvas.SetTop(marker, 30);
            timelineCanvas.Children.Add(marker);

            // 툴팁 추가
            marker.ToolTip = $"{FormatTime(timeInSeconds)}: {text}";

            _reminderMarkers[timeInSeconds] = marker;
        }

        /// <summary>
        /// 시간 포맷 (MM:SS)
        /// </summary>
        private string FormatTime(int seconds)
        {
            int minutes = seconds / 60;
            int secs = seconds % 60;
            return string.Format("{0:D2}:{1:D2}", minutes, secs);
        }

        #region Event Handlers

        /// <summary>
        /// 시간 업데이트 이벤트 핸들러
        /// </summary>
        private void OnTimeUpdated(object sender, int timeInSeconds)
        {
            txtCurrentTime.Text = FormatTime(timeInSeconds);
            UpdatePlayheadPosition(timeInSeconds);
        }

        /// <summary>
        /// 리마인더 트리거 이벤트 핸들러
        /// </summary>
        private void OnReminderTriggered(object sender, ReminderItem reminder)
        {
            // 토스트 알림 표시
            ToastNotification.Show(
                $"⏰ Reminder at {reminder.GetFormattedTime()}",
                reminder.Text,
                5 // 5초 동안 표시
            );
        }

        /// <summary>
        /// 상태 변경 이벤트 핸들러
        /// </summary>
        private void OnStateChanged(object sender, TimelineState state)
        {
            switch (state)
            {
                case TimelineState.Playing:
                    btnPlay.IsEnabled = false;
                    btnPause.IsEnabled = true;
                    break;

                case TimelineState.Paused:
                case TimelineState.Stopped:
                    btnPlay.IsEnabled = true;
                    btnPause.IsEnabled = false;
                    break;

                case TimelineState.Completed:
                    btnPlay.IsEnabled = false;
                    btnPause.IsEnabled = false;
                    MessageBox.Show("Timeline completed!", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        /// <summary>
        /// Play 버튼 클릭
        /// </summary>
        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            _timelineManager.Play();
        }

        /// <summary>
        /// Pause 버튼 클릭
        /// </summary>
        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            _timelineManager.Pause();
        }

        /// <summary>
        /// Reset 버튼 클릭
        /// </summary>
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            _timelineManager.Reset();
        }

        #endregion
    }
}
