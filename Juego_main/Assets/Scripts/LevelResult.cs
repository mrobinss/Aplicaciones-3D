// Resultado del ultimo nivel jugado, para mostrarlo en WinScene / LoseScene.
public static class LevelResult
{
    public static bool HasResult;
    public static int DeliveredOrders;
    public static int Stars;
    public static bool Won;

    public static void Set(int deliveredOrders, int stars, bool won)
    {
        HasResult = true;
        DeliveredOrders = deliveredOrders;
        Stars = stars;
        Won = won;
    }
}
