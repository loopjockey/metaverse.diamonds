<template>
  <v-bottom-sheet v-model="walletCheck" persistent inset no-click-animation>
    <v-divider></v-divider>
    <v-stepper v-model="currentStep">
      <v-stepper-header>
        <v-stepper-step :complete="currentStep > 1" step="1">
          Connect Wallet
        </v-stepper-step>

        <v-divider></v-divider>

        <v-stepper-step :complete="currentStep > 2" step="2">
          Sign Login Message
        </v-stepper-step>

        <v-divider></v-divider>

        <v-stepper-step step="3"> Terms and Conditions </v-stepper-step>
      </v-stepper-header>

      <v-stepper-items>
        <v-stepper-content step="1">
          <empty-state icon="account_balance_wallet">
            <h3>Step 1. Connect Wallet</h3>
            <p>
              Please connect your Ethereum wallet to continue. If you don't
              already have one already please checkout this page about
              <a href="https://ethereum.org/en/wallets/" target="_blank"
                >Ethereum wallets</a
              >.
            </p>
            <v-btn color="primary" x-large @click="$eth.connect()">
              <v-icon left>account_balance_wallet</v-icon> Connect Wallet</v-btn
            >
          </empty-state>
        </v-stepper-content>

        <v-stepper-content step="2">
          <empty-state icon="login">
            <h3>Step 2. Sign Message</h3>
            <p>Please sign a message to prove that you control the address.</p>
            <v-btn color="primary" x-large @click="getEthereumSignature()">
              <v-icon left>login</v-icon> Sign Login Message</v-btn
            >
          </empty-state>
        </v-stepper-content>

        <v-stepper-content step="3">
          <empty-state icon="login">
            <h3>Step 3. Accept Terms and Conditions</h3>
            <p>
              Finally please review the terms and conditions and continue if you
              accept them:
            </p>
            <v-checkbox
              v-model="acceptTermsAndConditions"
              label="Accept"
            ></v-checkbox>
            <v-btn
              color="primary"
              x-large
              @click="walletCheck = false"
              :disabled="!acceptTermsAndConditions"
            >
              <v-icon left>done</v-icon> Accept
            </v-btn>
          </empty-state>
        </v-stepper-content>
      </v-stepper-items>
    </v-stepper>
  </v-bottom-sheet>
</template>

<script>
import Api from "@/lib/Api.ts";
import EmptyState from "@/components/Common/EmptyState.vue";
import { waitFor } from "vue-wait";
export default {
  props: ["value", "user", "loading"],
  components: { EmptyState },
  data() {
    return {
      walletCheck: false,
      currentStep: 1,
      acceptTermsAndConditions: false,
    };
  },
  created() {
    this.$eth.on("connected", () => {
      this.currentStep = 2;
    });
  },
  mounted() {
    this.walletCheck = !this.value;
  },
  methods: {
    getEthereumSignature: waitFor("getEthereumSignature", async function () {
      const web3 = this.$eth.web3;
      const accounts = await web3.eth.getAccounts();
      const account = accounts[0];
      const user = this.user.name;
      const expiry = Api.authData.expiryTime?.toDate().getTime();
      const message = `I agree to link this user ${user} to my current address. Expires: ${expiry}`;
      const signedMessage = await web3.eth.personal.sign(message, account);
      Api.applyEthereumCredentials(signedMessage);
      Api.authData.currentAddress = account;
      this.$emit("input", account);
      this.currentStep = 3;
    }),
  },
};
</script>