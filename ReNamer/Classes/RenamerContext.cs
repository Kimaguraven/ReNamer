using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ReNamer
{
    public class RenamerContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<FileProperty> FileList { get; private set; }
        public string FormatText { get; set; }
        public string ReplaceSearchText { get; set; }
        public string ReplacedText { get; set; }
        public string SerialNumText { get; set; }
        public string SerialNumDigit { get; set; }

        private bool enabledUndo = false;
        public bool EnabledUndo
        {
            get { return enabledUndo; }
            set {
                enabledUndo = value;
                OnPropertyChanged(nameof(EnabledUndo));
            }
        }

        public RenamerContext()
        {
            FormatText = "{0}";
            FileList = new ObservableCollection<FileProperty>();
        }

        public void AddFile(string path)
        {
            for (int i = 0; i < FileList.Count; ++i)
            {
                // 同じパスの要素は登録しない
                if (FileList[i].Path == path) return;
            }
            FileList.Add(new FileProperty(path));
        }

        public void RemoveFile(FileProperty prop)
        {
            FileList.Remove(prop);
        }

        public void RemoveFiles(FileProperty[] props)
        {
            for (int i = 0; i < props.Length; ++i)
            {
                if (props[i] == null) continue;
                RemoveFile(props[i]);
            }
        }

        public void RemoveAllFiles()
        {
            FileList.Clear();
        }

        public void RefreshList()
        {
            for (int i = 0; i < FileList.Count; ++i)
            {
                var temp = FileList[i];
                FileList[i] = temp;
            }
        }

        public void RefreshUndoButton()
        {
            bool existHistory = false;
            if (FileList.Count > 0)
            {
                for (int i = 0; i < FileList.Count; ++i)
                {
                    var fileProp = FileList[i];
                    if (fileProp.History.Count > 0)
                    {
                        existHistory = true;
                        break;
                    }
                }
            }
            EnabledUndo = existHistory;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class FileProperty
    {
        public string Path { get; set; }
        public Stack<string> History { get; }

        public FileProperty(string path)
        {
            Path = path;
            History = new Stack<string>();
        }
    }
}
