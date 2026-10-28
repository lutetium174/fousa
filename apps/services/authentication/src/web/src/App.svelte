<script>
  import axios from "axios";
  import QRCode from "qrcode";
  import { onMount } from "svelte";

  const baseUrl = import.meta.env.VITE_API_BASE_URL || "https://localhost:5001";

  let qrCode = '';        // QR code PNG data URL generated client-side
  let requestUrl = '';   // raw openid4vp:// request URL, for installed wallets
  let state = '';         // login session ID
  let authenticated = false;
  let loading = false;
  let error = '';

  let pendingRequest = ''; // request received via the wallet entry (?request=)
  let presenting = false;
  let walletResult = '';

  onMount(() => {
    const request = new URLSearchParams(window.location.search).get("request");
    if (request) pendingRequest = request;
  });

  async function startLogin() {
    loading = true;
    error = '';
    authenticated = false;
    qrCode = '';
    requestUrl = '';

    try {
      const res = await axios.post(`${baseUrl}/api/auth/verifier/request`);
      const data = res.data;

      requestUrl = data.requestUrl;
      const entryUrl = `${window.location.origin}/?request=${encodeURIComponent(requestUrl)}`;
      qrCode = await QRCode.toDataURL(entryUrl, { width: 260 });
      state = data.state;

      //pollStatus();
    } catch (e) {
      error = "Failed to start wallet login.";
      console.error(e);
    } finally {
      loading = false;
    }
  }

  function pollStatus() {
    const interval = setInterval(async () => {
      try {
        const res = await axios.get(
          `${baseUrl}/api/auth/verifier/status/${state}`
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

  async function approveRequest() {
    presenting = true;
    walletResult = '';
    try {
      await axios.post(`${baseUrl}/api/auth/wallet/present`, { requestUrl: pendingRequest });
      walletResult = "Identity shared. You can return to the other device.";
      pendingRequest = '';
    } catch (e) {
      walletResult = "Could not answer the request. Does your wallet hold a matching credential?";
      console.error(e);
    } finally {
      presenting = false;
    }
  }

  function declineRequest() {
    pendingRequest = '';
    walletResult = "Request declined.";
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

  button + button {
    margin-left: 10px;
  }

  .same-device {
    display: inline-block;
    margin-top: 14px;
    padding: 10px 20px;
    border: 1px solid #2a6ea6;
    border-radius: 6px;
    color: #2a6ea6;
    text-decoration: none;
    font-size: 14px;
  }

  .payload {
    margin-top: 10px;
    padding: 8px;
    background: #f5f5f5;
    border-radius: 4px;
    font-family: monospace;
    font-size: 11px;
    word-break: break-all;
    text-align: left;
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
  <h1>Fousa DID Wallet</h1>

  {#if pendingRequest}
    <h2>Prove your identity</h2>
    <p>A service is asking you to present a credential from your wallet.</p>
    <button on:click={approveRequest} disabled={presenting}>
      {#if presenting}Sharing…{/if}
      {#if !presenting}Share credential{/if}
    </button>
    <button on:click={declineRequest} disabled={presenting}>Decline</button>
  {:else}
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
      <h2>Scan with your wallet</h2>
      <img src={qrCode} alt="Wallet request QR code" />
      <a class="same-device" href={requestUrl}>Use an installed wallet on this device</a>
      <div class="payload">{requestUrl}</div>
    {/if}

    {#if authenticated}
      <div class="success">You're logged in with your wallet</div>
    {/if}
  {/if}

  {#if walletResult}
    <div class="success">{walletResult}</div>
  {/if}
</div>
