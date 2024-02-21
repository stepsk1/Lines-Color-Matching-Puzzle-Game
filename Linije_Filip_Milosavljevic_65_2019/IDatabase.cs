using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linije_Filip_Milosavljevic_65_2019
{
    public interface IDatabase
    {
        Score GetBestScore();
        void InsertScore(int score, int time);
    }
}
