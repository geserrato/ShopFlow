namespace CatalogoService.Auth;

// In-memory — en producción usar Redis o tabla en BD
public class TokenRevocationStore
{
    private readonly HashSet<string> _revocados = [];
    private readonly Lock _lock = new();

    public void Revocar(string jti) { lock (_lock) _revocados.Add(jti); }
    public bool EstaRevocado(string jti) { lock (_lock) return _revocados.Contains(jti); }
}
