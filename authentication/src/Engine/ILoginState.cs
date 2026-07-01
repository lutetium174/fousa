namespace Engine;

public interface ILoginState
{
    public void SetState(Guid state, bool isValid);
    public bool GetState(Guid state);
}