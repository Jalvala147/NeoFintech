using ProyectoIndursa.IndursaContext;

namespace ProyectoIndursa.AccountFunctions
{
    public partial class Account
    {
        private static int NewID()
        {
            var db=new IndursaDB();
            Random random=new Random();
            int NoCuenta;
            IQueryable<int> exist;
            do{
            NoCuenta=random.Next(10000,99999);
            exist=from s in db.Cuenta where s.NoCuenta==NoCuenta select s.NoCuenta;
            }while(exist.Count()>0);
            return NoCuenta;
        }
    }
}