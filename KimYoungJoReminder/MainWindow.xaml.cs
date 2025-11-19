using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using KimYoungJoReminder.Models;
using KimYoungJoReminder.Pages;
using KimYoungJoReminder.Components;

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
        /// 리마인더 마커 리스트
        /// </summary>
        private List<ReminderMarker> _reminderMarkers;

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
            _reminderMarkers = new List<ReminderMarker>();

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
        private void UpdatePlayheadPosition()
        {
            // Stopwatch 기반 실시간 위치 계산
            double totalSeconds = _timelineManager.GetCurrentTimeInSecondsWithDecimal();
            double targetX = totalSeconds * PIXELS_PER_SECOND;

            // 애니메이션 취소하고 직접 위치 설정 (부드러운 업데이트는 0.1초 간격으로 자동)
            _playheadLine.BeginAnimation(Line.X1Property, null);
            _playheadLine.BeginAnimation(Line.X2Property, null);
            _playheadLine.X1 = targetX;
            _playheadLine.X2 = targetX;
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
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 45))
            };

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 시간 표시
            var timeLabel = new TextBlock
            {
                Text = $"Start Time: {FormatTime(timeInSeconds)}",
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

            // Duration 입력
            var durationPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            var durationLabel = new TextBlock
            {
                Text = "Duration (seconds):",
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            var durationTextBox = new TextBox
            {
                Text = "5",
                Width = 60,
                Padding = new Thickness(5)
            };
            durationPanel.Children.Add(durationLabel);
            durationPanel.Children.Add(durationTextBox);
            Grid.SetRow(durationPanel, 2);
            grid.Children.Add(durationPanel);

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
                    int duration = 5;
                    int.TryParse(durationTextBox.Text, out duration);
                    if (duration < 1) duration = 1;

                    if (_timelineManager.AddReminder(timeInSeconds, textBox.Text, duration))
                    {
                        AddReminderMarker(timeInSeconds, textBox.Text, duration);
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
            Grid.SetRow(buttonPanel, 3);
            grid.Children.Add(buttonPanel);

            inputWindow.Content = grid;
            inputWindow.ShowDialog();
        }

        /// <summary>
        /// 타임라인에 리마인더 마커 추가
        /// </summary>
        private void AddReminderMarker(int timeInSeconds, string text, int duration)
        {
            // ReminderItem 찾기
            var reminderItem = _timelineManager.GetAllReminders()
                .FirstOrDefault(r => r.TimeInSeconds == timeInSeconds);

            if (reminderItem != null)
            {
                // ReminderMarker 컴포넌트 생성
                var marker = new ReminderMarker(reminderItem, timelineCanvas, _timelineManager, PIXELS_PER_SECOND);

                // 이벤트 구독
                marker.EditRequested += (s, e) => EditReminder(reminderItem);
                marker.DeleteRequested += (s, e) => DeleteReminder(reminderItem);

                _reminderMarkers.Add(marker);

                // Canvas 높이 동적 조정
                UpdateCanvasHeight();
            }
        }

        /// <summary>
        /// Canvas 높이를 Lane 개수에 맞게 동적으로 조정
        /// </summary>
        private void UpdateCanvasHeight()
        {
            const double TIME_LABEL_HEIGHT = 30;
            const double LANE_HEIGHT = 25;
            const double BOTTOM_MARGIN = 20;

            int maxLane = _timelineManager.GetMaxLane();
            double requiredHeight = TIME_LABEL_HEIGHT + ((maxLane + 1) * LANE_HEIGHT) + BOTTOM_MARGIN;

            // 최소 높이 200px 보장
            if (requiredHeight < 200)
                requiredHeight = 200;

            timelineCanvas.Height = requiredHeight;

            // Playhead 라인 높이도 업데이트
            if (_playheadLine != null)
            {
                _playheadLine.Y2 = requiredHeight;
            }
        }

        /// <summary>
        /// 리마인더 삭제
        /// </summary>
        private void DeleteReminder(ReminderItem reminderItem)
        {
            // TimelineManager에서 삭제
            if (_timelineManager.RemoveReminder(reminderItem.TimeInSeconds))
            {
                // UI에서 마커 제거
                var marker = _reminderMarkers.FirstOrDefault(m => m.Data == reminderItem);
                if (marker != null)
                {
                    marker.Remove();
                    _reminderMarkers.Remove(marker);

                    // Canvas 높이 재조정
                    UpdateCanvasHeight();
                }
            }
        }

        /// <summary>
        /// 리마인더 수정
        /// </summary>
        private void EditReminder(ReminderItem reminderItem)
        {
            if (reminderItem == null)
                return;

            // 입력 다이얼로그 표시
            var inputWindow = new Window
            {
                Title = "Edit Reminder",
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
                Text = $"Time: {FormatTime(reminderItem.TimeInSeconds)}",
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(timeLabel, 0);
            grid.Children.Add(timeLabel);

            // 텍스트 입력 (기존 텍스트로 초기화)
            var textBox = new TextBox
            {
                Text = reminderItem.Text,
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
                    // 기존 텍스트 업데이트
                    reminderItem.Text = textBox.Text;

                    // UI 마커의 툴팁 업데이트
                    var marker = _reminderMarkers.FirstOrDefault(m => m.Data == reminderItem);
                    if (marker != null)
                    {
                        marker.UpdateUI();
                    }

                    inputWindow.DialogResult = true;
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
            txtCurrentTime.Text = _timelineManager.GetFormattedCurrentTimeWithDecimal();
            UpdatePlayheadPosition();
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

        /// <summary>
        /// Boss Profile 선택 변경
        /// </summary>
        private void CmbBossProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 초기화 중에는 이벤트 무시
            if (cmbBossProfile == null || cmbBossProfile.SelectedItem == null)
                return;

            var selectedItem = cmbBossProfile.SelectedItem as ComboBoxItem;
            if (selectedItem != null && selectedItem.Content != null)
            {
                string selectedBoss = selectedItem.Content.ToString();

                // TODO: 선택된 보스에 따라 프로필 로드
            }
        }

        #endregion
    }
}
