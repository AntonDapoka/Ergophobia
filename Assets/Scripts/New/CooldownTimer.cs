
public class CooldownTimer
{
    private float currentCooldown = 0f;

    public void SetCooldown(float duration)
    {
        currentCooldown = duration;
    }

    public void Tick(float deltaTime)
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= deltaTime;
        }
    }

    public bool IsReady()
    {
        return currentCooldown <= 0f;
    }

    public void Reset()
    {
        currentCooldown = 0f;
    }
}