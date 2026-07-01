import { useNavigate } from '@solidjs/router';
import {
  Container,
  Box,
  Button,
  Typography,
  Paper,
  Divider,
  Stack,
} from '@suid/material';
import { LogoutOutlined } from '@suid/icons-material';
import { useAuth } from '../contexts/authContext';

export default function HomePage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <Container maxWidth="md">
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          minHeight: '100vh',
          py: 4,
        }}
      >
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            mb: 3,
          }}
        >
          <Typography variant="h4" component="h1" sx={{ fontWeight: 600 }}>
            Welcome
          </Typography>
          <Button
            variant="contained"
            color="error"
            endIcon={<LogoutOutlined />}
            onClick={handleLogout}
          >
            Logout
          </Button>
        </Box>

        <Stack spacing={3}>
          <Paper
            elevation={2}
            sx={{
              p: 3,
              borderRadius: 2,
            }}
          >
            <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
              Account Information
            </Typography>
            <Divider sx={{ mb: 2 }} />
            <Stack spacing={1}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                <Typography variant="body2" color="textSecondary">
                  <strong>Username:</strong>
                </Typography>
                <Typography variant="body2">{user()?.username}</Typography>
              </Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                <Typography variant="body2" color="textSecondary">
                  <strong>User ID:</strong>
                </Typography>
                <Typography variant="body2" sx={{ wordBreak: 'break-all' }}>
                  {user()?.userId}
                </Typography>
              </Box>
              {user()?.languagePreference && (
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" color="textSecondary">
                    <strong>Language Preference:</strong>
                  </Typography>
                  <Typography variant="body2">{user()?.languagePreference}</Typography>
                </Box>
              )}
            </Stack>
          </Paper>

          <Paper
            elevation={2}
            sx={{
              p: 3,
              borderRadius: 2,
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            }}
          >
            <Typography variant="h6" sx={{ mb: 2, fontWeight: 600, color: 'white' }}>
              You're All Set!
            </Typography>
            <Typography variant="body2" sx={{ mb: 1.5, color: 'rgba(255, 255, 255, 0.9)' }}>
              Your account has been verified with WaltID. You now have access to the platform.
            </Typography>
            <Typography variant="body2" sx={{ color: 'rgba(255, 255, 255, 0.9)' }}>
              Start using the application to manage your digital credentials and identity.
            </Typography>
          </Paper>
        </Stack>
      </Box>
    </Container>
  );
}
