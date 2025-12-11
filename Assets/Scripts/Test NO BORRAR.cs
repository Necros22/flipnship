using UnityEngine;

public class Test : MonoBehaviour
    //Este script era para probar los botones, si se borra es posible que se caiga todo
{
    private int test4Counter = 0;
    private bool test4Blocked = false;
    public void test1()
    {
        print("Botón 1 Funcional");
    }

    public void test2()
    {
        print("Botón multiple Funcional");
    }

    public void test3()
    {
        print("Botón multiple 2 Funcional");
    }

    public void test4()
    {
        if (test4Blocked)
            return;

        test4Counter++;
        print("Botón de reinicio Funcional");
        print("Contador: " + test4Counter);

        if (test4Counter >= 6)
        {
            test4Blocked = true;
            print("Los botones se han presionado 6 veces");
        }
    }
}
