import { createSignal, Show } from 'solid-js';
import { useNavigate } from '@solidjs/router';
import {
  Container,
  Box,
  TextField,
  Button,
  Typography,
  Paper,
  Alert,
  CircularProgress,
  Link as MuiLink,
  ButtonGroup,
} from '@suid/material';
import { useAuth } from '../contexts/authContext';
import { authService } from '../services/authService';

type LoginMode = 'login' | 'recover';

export default function LoginPage() {
  const [username, setUsername] = createSignal('');
  const [mode, setMode] = createSignal<LoginMode>('login');
  const { login, recover, isLoading, error, clearError } = useAuth();
  const navigate = useNavigate();

  const handleLogin = async (e: Event) => {
    e.preventDefault();
    clearError();

    try {
      await login(username());
      navigate('/');
    } catch (err) {
      // Error is handled by context and displayed via error signal
    }
  };

  const signChallengeWithWallet = async (challenge: string) => {
    const anyWindow: any = window as any;
    if (anyWindow?.ethereum) {
      const accounts = await anyWindow.ethereum.request({ method: 'eth_requestAccounts' });
      const address = accounts[0];
      const signature = await anyWindow.ethereum.request({ method: 'personal_sign', params: [challenge, address] });
      return { signature, address };
    }

    const signature = prompt('No compatible wallet detected. Please sign the challenge in your wallet and paste the signature here.');
    return { signature: signature || '', address: undefined };
  };

  const handleModeChange = (newMode: LoginMode) => {
    setMode(newMode);
    clearError();
    setUsername('');
  };

  const handleRecover = async () => {
    clearError();
    try {
      const init = await authService.recoverInitiate(username());
      const { signature, address } = await signChallengeWithWallet(init.challenge);
      if (!signature) throw new Error('No signature provided');
      await recover(username(), signature, address);
      navigate('/');
    } catch (err) {
      // Error handled by context
    }
  };

  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          alignItems: 'center',
          minHeight: '100vh',
          py: 4,
        }}
      >
        <Paper
          elevation={3}
          sx={{
            p: 4,
            width: '100%',
            borderRadius: 2,
          }}
        >
          <Typography variant="h4" component="h1" sx={{ mb: 0.5, fontWeight: 600 }}>
            Access Account
          </Typography>
          <Typography variant="subtitle1" color="textSecondary" sx={{ mb: 2 }}>
            Verify with WaltID
          </Typography>

          {error() && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error()}
            </Alert>
          )}

          <ButtonGroup fullWidth sx={{ mb: 3 }}>
            <Button
              variant={mode() === 'login' ? 'contained' : 'outlined'}
              onClick={() => handleModeChange('login')}
              disabled={isLoading()}
            >
              Login
            </Button>
            <Button
              variant={mode() === 'recover' ? 'contained' : 'outlined'}
              onClick={() => handleModeChange('recover')}
              disabled={isLoading()}
            >
              Recover Account
            </Button>
          </ButtonGroup>

          <Show when={mode() === 'login'}>
            <Box component="form" onSubmit={handleLogin} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <TextField
                id="username-login"
                label="Username"
                placeholder="Enter your username"
                fullWidth
                value={username()}
                onChange={(e) => setUsername((e.target as HTMLInputElement).value)}
                disabled={isLoading()}
                required
                variant="outlined"
              />

              <Button
                type="submit"
                fullWidth
                variant="contained"
                disabled={isLoading() || !username()}
                sx={{ mt: 1 }}
              >
                {isLoading() ? (
                  <>
                    <CircularProgress size={20} sx={{ mr: 1 }} />
                    Verifying...
                  </>
                ) : (
                  'Login'
                )}
              </Button>
            </Box>
          </Show>

          <Show when={mode() === 'recover'}>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <TextField
                id="username-recover"
                label="Username"
                placeholder="Enter your username"
                fullWidth
                value={username()}
                onChange={(e) => setUsername((e.target as HTMLInputElement).value)}
                disabled={isLoading()}
                required
                variant="outlined"
              />

              <Typography variant="body2" color="textSecondary">
                Recover by proving control of your recovery wallet.
              </Typography>

              <Button
                fullWidth
                variant="contained"
                disabled={isLoading() || !username()}
                onClick={handleRecover}
              >
                {isLoading() ? (
                  <>
                    <CircularProgress size={20} sx={{ mr: 1 }} />
                    Recovering...
                  </>
                ) : (
                  'Recover via Wallet'
                )}
              </Button>
            </Box>
          </Show>

          <Typography variant="body2" sx={{ mt: 2, textAlign: 'center' }}>
            Don't have an account?{' '}
            <MuiLink href="/register" underline="none" sx={{ fontWeight: 500, cursor: 'pointer' }}>
              Create one here
            </MuiLink>
          </Typography>
        </Paper>
      </Box>
    </Container>
  );
}
