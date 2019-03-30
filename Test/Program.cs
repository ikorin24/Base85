using System;
using System.Text;
using Base85Project;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WindowWidth = 120;
            var dataList = new string[]
            {
                "吾輩は猫である", "Man sure.", ";lkajeavaop    20", "わ", "ho2@", "Bito", "af", "ge1", "345r", "03k5me", "+++++++",
                "hあ；ぇそれってエア", "だｊだおいうに", "変更会おうでもｓれ；ｌ", "hoie"
            };

            foreach(var data in dataList) {
                var base85 = Encoding.ASCII.GetString(Base85.Encode(Encoding.UTF8.GetBytes(data)));
                var data2 = Encoding.UTF8.GetString(Base85.Decode(Encoding.ASCII.GetBytes(base85)));
                Console.WriteLine($"{base85.PadRight(50)} {data2.PadRight(30)} {data == data2}");
            }
            Console.ReadKey();
        }
    }
}
