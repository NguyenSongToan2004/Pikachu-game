using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlay
{
    public class PlayerScore
    {
        public string PlayerName { get; set; } // Thêm thuộc tính tên người chơi
        public int Score { get; set; }

        public override string ToString()
        {
            return $"{PlayerName}: {Score}";
        }
    }
}
