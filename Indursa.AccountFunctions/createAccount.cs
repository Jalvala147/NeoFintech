using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ProyectoIndursa.Models;
using ProyectoIndursa.IndursaContext;

namespace ProyectoIndursa.AccountFunctions
{
    public partial class Account
    {
        public static void CreateAccount(Usuario user)
        {
            
            var db=new IndursaDB();
            int newID=Account.NewID();
            Cuentum cuenta=new Cuentum
            {
                NoCuenta=newID,
                TipoCuenta=1,
            };
            InfoCuentum infocuenta=new InfoCuentum
            {
                NoCuenta=newID
            };
            user.NoCuenta=newID;
            db.Cuenta.Add(cuenta);
            db.InfoCuenta.Add(infocuenta);
            db.Usuarios.Add(user);
            db.SaveChanges();
        }
    }
}
