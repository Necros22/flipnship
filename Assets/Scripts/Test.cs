using UnityEngine;

public class Test : MonoBehaviour
{
    private int test4Counter = 0;    // ← contador interno
    private bool test4Blocked = false; // ← bloqueo permanente

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
            return; // Ya no hace nada

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
