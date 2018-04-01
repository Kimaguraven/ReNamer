using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace ReNamer
{
    /// <summary>
    /// アプリケーション機能クラス
    /// </summary>
    public class RenamerCore
    {
        public enum RenameResult
        {
            Done,
            Failed,
            EmptyName,
            SameFilePath,
        }

        RenamerContext context;

        public RenamerCore(RenamerContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// ファイルの追加読み込み
        /// </summary>
        public void OpenFiles()
        {
            var fileDialog = new OpenFileDialog() { Title = "ファイルの読み込み", Multiselect = true};
            if (fileDialog.ShowDialog() == true)
            {
                var loadFiles = fileDialog.FileNames;
                for (int i = 0; i < loadFiles.Length; ++i)
                {
                    context.AddFile(loadFiles[i]);
                }
            }
        }

        /// <summary>
        /// リネーム開始
        /// </summary>
        public void StartRename(FileProperty[] fileList)
        {
            bool isFailed = false;
            bool isFileLost = false;
            for (int i = 0; i < fileList.Length; ++i)
            {
                var fileProp = fileList[i];
                if (fileProp == null) continue;
                
                // ファイルの有無確認
                if (!File.Exists(fileProp.Path))
                {
                    // 存在しない場合はリストから除外する
                    context.RemoveFile(fileProp);
                    isFileLost = true;
                    continue;
                }
                
                var result = Rename(fileList[i], i);
                if (result == RenameResult.Failed)
                {
                    isFailed = true;
                    break;
                }
                else if (result == RenameResult.EmptyName)
                {

                }
            }
            
            context.RefreshList();
            context.RefreshUndoButton();

            if (isFailed) return;

            //完了のダイアログ表示
            if (isFileLost == false)
            {
                MessageBox.Show("リネームが完了しました", "確認", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show("リネームが完了しました\n一部のファイルが移動または削除されていたため\nリストから除外しました", "確認", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        // TODO: リターンで処理の結果をEnumで返す
        /// <summary>
        /// リネーム処理
        /// </summary>
        /// <param name="fileProp"></param>
        /// <param name="index"></param>
        private RenameResult Rename(FileProperty fileProp, int index)
        {
            string[] splitPathes = fileProp.Path.Split(Path.DirectorySeparatorChar);
            string fileName, oldFilePath, extention;
            oldFilePath = fileProp.Path;
            extention = Path.GetExtension(fileProp.Path);
            fileName = Path.GetFileNameWithoutExtension(fileProp.Path);

            // 置換処理
            if (!string.IsNullOrEmpty(context.ReplaceSearchText))
            {
                fileName = fileName.Replace(context.ReplaceSearchText, context.ReplacedText);
            }

            // 連番処理
            string serialNumber = "";
            if (!string.IsNullOrEmpty(context.SerialNumText))
            {
                if (int.TryParse(context.SerialNumText, out int num) && int.TryParse(context.SerialNumDigit, out int digit))
                {
                    num += index;
                    serialNumber = num.ToString("d" + digit.ToString());
                }
                else
                {
                    // テキストの指定エラーのダイアログ表示
                    MessageBox.Show("連番指定には必ず半角数字を入力してください", "確認", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return RenameResult.Failed;
                }
            }

            // リネーム
            fileName = context.FormatText.Replace("{0}", fileName);
            fileName = fileName.Replace("{1}", serialNumber);
            if (string.IsNullOrEmpty(fileName)) return RenameResult.EmptyName;
            
            // 変更後の絶対パス作成
            string renamedFilePath = splitPathes[0];
            for (int i = 1; i < splitPathes.Length - 1; ++i)
            {
                renamedFilePath += Path.DirectorySeparatorChar + splitPathes[i];
            }
            renamedFilePath += Path.DirectorySeparatorChar + fileName + extention;

            // ファイルのリネーム実行
            if (renamedFilePath == fileProp.Path) return RenameResult.Done;
            if (File.Exists(renamedFilePath)) return RenameResult.SameFilePath;

            File.Move(fileProp.Path, renamedFilePath);

            fileProp.History.Push(oldFilePath);
            fileProp.Path = renamedFilePath;

            return RenameResult.Done;
        }

        /// <summary>
        /// 直前のリネームを元に戻す
        /// </summary>
        public void UndoRename()
        {
            var fileList = context.FileList;
            for (int i = 0; i < fileList.Count; ++i)
            {
                var fileProp = fileList[i];
                if (File.Exists(fileProp.Path) && fileProp.History.Count > 0)
                {
                    string undoPath = fileProp.History.Pop();
                    if (!File.Exists(undoPath))
                    {
                        File.Move(fileProp.Path, undoPath);
                        fileProp.Path = undoPath;
                    }
                }
            }

            context.RefreshList();
            context.RefreshUndoButton();

            MessageBox.Show("リネーム処理を元に戻しました", "確認", MessageBoxButton.OK);
        }
    }
}
