using Engine;

namespace Did.WaltId.Wallet;

public class LoginState: ILoginState
{
    private readonly Dictionary<Guid, bool> _stateDictionary = new();

    public void SetState(Guid state, bool isValid) => _stateDictionary[state] = isValid;

    public void SetState(string key, bool state)
    {
        throw new NotImplementedException();
    }

    public bool GetState(Guid state) => _stateDictionary.TryGetValue(state, out var isValid) && isValid;
}