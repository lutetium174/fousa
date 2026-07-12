import { createSignal } from 'solid-js';
import { useNavigate } from '@solidjs/router';
import {
  Container,
  Box,
  TextField,
  Button,
  Typography,
  Paper,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  CircularProgress,
  Link as MuiLink,
} from '@suid/material';
import { useAuth } from '../contexts/authContext';

export default function RegisterPage() {
  const [username, setUsername] = createSignal('');
  const [languagePreference, setLanguagePreference] = createSignal('en');
  const { register, isLoading, error, clearError } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: Event) => {
    e.preventDefault();
    clearError();

    try {
      await register(username(), languagePreference());
      navigate('/');
    } catch (err) {
      // Error is handled by context and displayed via error signal
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
            Create Account
          </Typography>
          <Typography variant="subtitle1" color="textSecondary" sx={{ mb: 2 }}>
            Register with WaltID
          </Typography>

          {error() && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error()}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField
              id="username"
              label="Username"
              placeholder="Choose a username"
              fullWidth
              value={username()}
              onChange={(e) => setUsername((e.target as HTMLInputElement).value)}
              disabled={isLoading()}
              required
              variant="outlined"
            />

            <FormControl fullWidth>
              <InputLabel id="language-label">Language Preference</InputLabel>
              <Select
                labelId="language-label"
                id="language"
                value={languagePreference()}
                label="Language Preference"
                onChange={(e) => setLanguagePreference(e.target.value)}
                disabled={isLoading()}
              >
                <MenuItem value="en">English</MenuItem>
                <MenuItem value="es">Español</MenuItem>
                <MenuItem value="fr">Français</MenuItem>
                <MenuItem value="de">Deutsch</MenuItem>
                <MenuItem value="it">Italiano</MenuItem>
                <MenuItem value="pt">Português</MenuItem>
              </Select>
            </FormControl>

            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 1 }}
              disabled={isLoading() || !username()}
            >
              {isLoading() ? (
                <>
                  <CircularProgress size={20} sx={{ mr: 1 }} />
                  Creating Account...
                </>
              ) : (
                'Register'
              )}
            </Button>
          </Box>

          <Typography variant="body2" sx={{ mt: 2, textAlign: 'center' }}>
            Already have an account?{' '}
            <MuiLink href="/login" underline="none" sx={{ fontWeight: 500, cursor: 'pointer' }}>
              Sign in here
            </MuiLink>
          </Typography>

          <Alert severity="info" sx={{ mt: 2 }}>
            <Typography variant="body2">
              <strong>Important:</strong> Your recovery wallet association will be configured after account creation.
            </Typography>
          </Alert>
        </Paper>
      </Box>
    </Container>
  );
}
