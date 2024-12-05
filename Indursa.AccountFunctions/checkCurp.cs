using ProyectoIndursa.IndursaContext;
using ProyectoIndursa.Models;
using System;

namespace ProyectoIndursa.AccountFunctions
{
    public partial class Account
    {
        public static bool checkCurp(Usuario user)
        {
            var db = new IndursaDB();
            var exist = from s in db.Usuarios where s.Curp == user.Curp select s.Curp;
            if (exist.Count() > 0)
            {
                return false;
            }
            else{
                return true;
            }
            // if (user.Curp[0] == user.ApellidoPaterno[0] && user.Curp[1] == user.ApellidoPaterno[1] && user.Curp[2]==user.ApellidoMaterno[0] && user.Curp[3]==user.Nombre[0])
            // {   
            //     return true;
            // }
            // else
            // {
            //     return false;
            // }
        }

        // private static int checkVowel(string word)
        // {
        //     string vowels = "aeiou";
        //     string loweredWord = word.ToLower();

        //     for (int index = 1; index < loweredWord.Length; index++)
        //     {
        //         if (vowels.Contains((loweredWord[index])))
        //         {
        //             return index;
        //         }
        //     }
        //     return -1;
        // }
    }
}