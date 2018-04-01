using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ReNamer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        RenamerContext context;
        RenamerCore core;

        public MainWindow()
        {
            InitializeComponent();

            // コンテキストの設定
            context = new RenamerContext();
            FileListBox.DataContext = context;
            FormatText.DataContext = context;
            ReplaceSearchText.DataContext = context;
            ReplacedText.DataContext = context;
            SerialNumberText.DataContext = context;
            DigitNumberText.DataContext = context;
            UndoButton.DataContext = context;

            // 基本機能の作成
            core = new RenamerCore(context);
        }

        /// <summary>
        /// ファイル追加ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickAdd(object sender, RoutedEventArgs e)
        {
            core.OpenFiles();
        }

        /// <summary>
        /// ファイル除外ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickRemove(object sender, RoutedEventArgs e)
        {
            var selectedList = new FileProperty[FileListBox.SelectedItems.Count];
            FileListBox.SelectedItems.CopyTo(selectedList, 0);

            context.RemoveFiles(selectedList);
            context.RefreshUndoButton();
        }

        /// <summary>
        /// 全ファイル除外ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickAllRemove(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("対象ファイル全てをリストから除外しますか？\n(ファイルの削除はされません)", "全ファイル除外の確認", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                context.RemoveAllFiles();
                context.RefreshUndoButton();
                MessageBox.Show("全ファイルを除外しました", "確認");
            }
        }

        /// <summary>
        /// ファイル全選択ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickAllSelect(object sender, RoutedEventArgs e)
        {
            FileListBox.SelectAll();
            FileListBox.Focus();
        }

        /// <summary>
        /// ファイル全選択解除ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickAllUnSelect(object sender, RoutedEventArgs e)
        {
            FileListBox.SelectedItems.Clear();
            FileListBox.Focus();
        }

        /// <summary>
        /// リネーム実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickRename(object sender, RoutedEventArgs e)
        {
            var fileList = new FileProperty[context.FileList.Count];
            context.FileList.CopyTo(fileList, 0);

            core.StartRename(fileList);
            FileListBox.Items.Refresh();
        }

        /// <summary>
        /// リネーム結果を元に戻す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickUndo(object sender, RoutedEventArgs e)
        {
            core.UndoRename();
            FileListBox.Items.Refresh();
        }

        /// <summary>
        /// リストに要素がドラッグされた時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// リストに要素がドロップされた時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDrop(object sender, DragEventArgs e)
        {
            var filePathes = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (filePathes == null) return;
            for (int i = 0; i < filePathes.Length; ++i)
            {
                if (File.Exists(filePathes[i]))
                    context.AddFile(filePathes[i]);
            }
        }

        /// <summary>
        /// メインウィンドウフォーカス時のボタン押下時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // リネーム実行の判定
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    OnClickRename(sender, e);
                    e.Handled = true;
                }
            }

            // 全ファイル除外の判定
            if (e.Key == Key.E)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    OnClickAllRemove(sender, e);
                    e.Handled = true;
                }
            }
            
            // 終了判定
            if (e.Key == Key.Escape)
            {
                var result = MessageBox.Show("終了しますか？", "アプリケーションの終了", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                    e.Handled = true;
                }
            }
        }
    }
}
