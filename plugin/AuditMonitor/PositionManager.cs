using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditMonitor
{
    class PositionManager
    {
        private List<LogLeaf> _LogLeafList = null;

        private bool _IsBottom = true;
        private int _CurrentLine = 0;
        private int _BottomLine = 0;

        public PositionManager()
        {
            this._LogLeafList = new List<LogLeaf>();
        }

        public void ClearView()
        {
            Console.Clear();
            _BottomLine = 0;
            _LogLeafList.Clear();
            _IsBottom = true;
        }

        public void AddAndView(string jsonText)
        {
            LogLeaf leaf = LogLeaf.FromJson(jsonText);
            if (leaf != null)
            {
                this._LogLeafList.Add(leaf);

                Console.CursorTop = _BottomLine;
                leaf.ViewLine();
                _BottomLine = Console.CursorTop;
                if (!_IsBottom)
                {
                    Console.CursorTop = _CurrentLine;
                }
            }
        }

        /// <summary>
        /// キー↑押下時動作
        /// </summary>
        public void MoveUp()
        {
            if (_IsBottom)
            {
                this._IsBottom = false;
                this._CurrentLine = _LogLeafList.Count;
            }
            else if (_CurrentLine == 0)
            {
                return;
            }
            _CurrentLine--;

            Console.Clear();

            for (int i = 0; i < _LogLeafList.Count; i++)
            {
                _LogLeafList[i].ViewLine();
                if (i == _CurrentLine)
                {
                    _LogLeafList[i].ViewDetail();
                }
            }
            _BottomLine = Console.CursorTop;
            Console.CursorTop = _CurrentLine;
        }

        /// <summary>
        /// キー↓押下時動作
        /// </summary>
        public void Movedown()
        {
            if (_IsBottom)
            {
                return;
            }
            else if (_CurrentLine == _LogLeafList.Count - 1)
            {
                _IsBottom = true;
            }
            _CurrentLine++;

            Console.Clear();

            for (int i = 0; i < _LogLeafList.Count; i++)
            {
                _LogLeafList[i].ViewLine();
                if (i == _CurrentLine)
                {
                    _LogLeafList[i].ViewDetail();
                }
            }
            _BottomLine = Console.CursorTop;
            Console.CursorTop = _CurrentLine;
        }

        /// <summary>
        /// Home押下時動作
        /// </summary>
        public void MoveHome()
        {
            this._IsBottom = false;
            this._CurrentLine = 0;

            Console.Clear();

            for (int i = 0; i < _LogLeafList.Count; i++)
            {
                _LogLeafList[i].ViewLine();
                if (i == _CurrentLine)
                {
                    _LogLeafList[i].ViewDetail();
                }
            }
            _BottomLine = Console.CursorTop;
            Console.CursorTop = _CurrentLine;
        }

        /// <summary>
        /// End押下時動作
        /// </summary>
        public void MoveEnd()
        {
            this._IsBottom = true;
            this._CurrentLine = _LogLeafList.Count;

            Console.Clear();

            for (int i = 0; i < _LogLeafList.Count; i++)
            {
                _LogLeafList[i].ViewLine();
                if (i == _CurrentLine)
                {
                    _LogLeafList[i].ViewDetail();
                }
            }
            _BottomLine = Console.CursorTop;
            Console.CursorTop = _CurrentLine;
        }
    }
}
