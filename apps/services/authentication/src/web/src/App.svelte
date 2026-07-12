<script>
  import axios from "axios";

  let qrCode = '';        // base64 PNG from backend
  let state = '';         // login session ID
  let authenticated = false;
  let loading = false;
  let error = '';
  let baseUrl = "http://localhost:5000";

  async function startLogin() {
    loading = true;
    error = '';
    authenticated = false;
    qrCode = '';

    try {
      const res = await axios.post(`${baseUrl}/api/verifier/request`);
      const data = res.data;

      qrCode = data.qrCode;
      state = data.state;

      pollStatus();
    } catch (e) {
      error = "Failed to start Verified ID login.";
      console.error(e);
    } finally {
      loading = false;
    }
  }

  function pollStatus() {
    const interval = setInterval(async () => {
      try {
        const res = await axios.get(
          `${baseUrl}/api/verifier/status/${state}`
        );

        if (res.data.authenticated) {
          authenticated = true;
          clearInterval(interval);
        }
      } catch (e) {
        console.error("Polling error:", e);
      }
    }, 2000);
  }
</script>

<style>
  .container {
    max-width: 600px;
    margin: auto;
    text-align: center;
    padding-top: 50px;
    font-family: system-ui, sans-serif;
  }

  img {
    margin-top: 20px;
    width: 260px;
    height: 260px;
    border: 1px solid #ddd;
    border-radius: 8px;
  }

  button {
    padding: 12px 24px;
    font-size: 16px;
    border-radius: 6px;
    cursor: pointer;
  }

  .error {
    color: #d33;
    margin-top: 10px;
  }

  .success {
    color: #2a8f2a;
    font-size: 22px;
    margin-top: 20px;
  }
</style>

<div class="container">
  <h1>Verified ID Login (Svelte + pnpm)</h1>

  {#if !qrCode && !authenticated}
    <button on:click={startLogin} disabled={loading}>
      {#if loading}Starting…{/if}
      {#if !loading}Start Login{/if}
    </button>
  {/if}

  {#if error}
    <div class="error">{error}</div>
  {/if}

  {#if qrCode}
    <h2>Scan with Microsoft Authenticator</h2>
    <img src={qrCode} alt="Verified ID QR Code" />
  {/if}

  {#if authenticated}
    <div class="success">🎉 You’re logged in with Verified ID</div>
  {/if}
</div>
