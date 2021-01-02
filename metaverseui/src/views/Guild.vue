<template>
  <fragment>
    <v-main>
      <v-dialog
        :value="true"
        width="600"
        persistent
        retain-focus
        hide-overlay
        no-click-animation
      >
        <v-card>
          <v-progress-linear
            indeterminate
            :active="requestingSignature"
          ></v-progress-linear>
          <v-stepper v-model="step" vertical>
            <v-stepper-step :complete="step > 1" step="1" complete-icon="done">
              Enter Token URL
            </v-stepper-step>

            <v-stepper-content step="1">
              <v-autocomplete
                v-model="selectedEntity"
                :search-input.sync="suppliedUrl"
                :items="items"
                :prepend-inner-icon="selectedEntity ? null : 'language'"
                :color="selectedEntity ? 'success' : 'primary'"
                hide-details
                filled
                :filter="() => true"
                label="ERC721 Token"
              >
                <template #no-data>
                  <v-container>
                    Please enter your Token URL. Examples of token URLs are:
                    <ul>
                      <li>https://app.rarible.com/token/0x...123:456</li>
                      <li>https://opensea.io/assets/0x...123/456</li>
                      <li>erc721://0x...123/456</li>
                      <li>or just the token ID 0x...123:456</li>
                    </ul>
                    If lost, please contact the owner of the discord server that
                    sent you this link.
                  </v-container>
                </template>
                <template #item>
                  <v-icon color="success" left>done</v-icon>
                  Valid URL! (Click to use)
                </template>
                <template #append v-if="selectedEntity">
                  <v-btn icon @click="selectedEntity = null" color="error">
                    <v-icon>close</v-icon>
                  </v-btn>
                </template>
              </v-autocomplete>
            </v-stepper-content>

            <v-stepper-step :complete="step > 2" step="2" complete-icon="done">
              Connect your web wallet (e.g. Metamask)
            </v-stepper-step>

            <v-stepper-content step="2">
              <v-btn color="primary" @click="connect()">
                <v-icon left>account_balance_wallet</v-icon>Connect
                Wallet</v-btn
              >
            </v-stepper-content>

            <v-stepper-step :complete="step > 3" step="3" complete-icon="done">
              Sign verification message
            </v-stepper-step>

            <v-stepper-content step="3">
              You need to sign a message to prove control of your Ethereum
              account.

              <br /><v-btn
                color="primary"
                @click="signVerificationMessage()"
                :disabled="!$eth.isConnected"
                class="mt-4"
              >
                <v-icon left>account_balance_wallet</v-icon>Sign Verification
                Message</v-btn
              >
            </v-stepper-content>

            <v-stepper-step step="4" complete-icon="done"
              >Share signature on discord</v-stepper-step
            >
            <v-stepper-content step="4">
              Please share the following in a direct message with the bot
              <code>Metaverse Diamonds#7589</code>. Alternatively if you are OK
              with other server participants being able to work out what your address is please
              share the following in any channel within the server.
              <v-textarea :value="discordCommand" filled class="pt-4" rows="7" ref="discordCommandText" @click="selectAllText">
              </v-textarea>
              <!--<v-card  class="mb-2 mt-4 pa-3">
                {{ discordCommand }}
              </v-card>-->
              <v-btn color="primary" @click="copyText()">
                <v-icon left>content_copy</v-icon>Copy</v-btn
              >
            </v-stepper-content>
          </v-stepper>
        </v-card>
      </v-dialog>
    </v-main>
  </fragment>
</template>

<script>
//https://app.rarible.com/token/0x729cd6226751279030757f61b2cac4798c949fa1:1:0x41a5afe15347fa975f2ddd9ec1aa1f55db835a52
export function tryParseRaribleLink(url) {
  if (!url.startsWith("https://app.rarible.com/token/")) return [false, null];
  const token = url.replace("https://app.rarible.com/token/", "");
  const parts = token.split(":");
  let creatorAddress = parts[0];
  const tokenId = parts[1];
  if (creatorAddress === "0xd07dc4262bcdbf85190c01c996b4c06a461d2430")
    creatorAddress = parts[2] || parts[0];
  return [true, `${creatorAddress}:${tokenId}`];
}

//https://opensea.io/assets/0xf90aeef57ae8bc85fe8d40a3f4a45042f4258c67/77
export function tryParseOpenSeaLink(url) {
  if (!url.startsWith("https://opensea.io/assets/")) return [false, null];
  const token = url.replace("https://opensea.io/assets/", "");
  const parts = token.split("/");
  const creatorAddress = parts[0];
  const tokenId = parts[1];
  return [true, `${creatorAddress}:${tokenId}`];
}

export function tryParseERC721Link(url) {
  if (!url.startsWith("erc721://")) return [false, null];
  const token = url.replace("erc721://", "");
  const parts = token.split("/");
  const creatorAddress = parts[0];
  const tokenId = parts[1];
  return [true, `${creatorAddress}:${tokenId}`];
}

export default {
  data() {
    return {
      drawer: true,
      selectedEntity: null,
      suppliedUrl: null,
      requestingSignature: false,
      items: [],
      step: 1,
      signature: null,
      signedMessage: null,
      signatureTimestamp: null,
      discordCommand: null,
    };
  },
  watch: {
    suppliedUrl(url) {
      this.items = this.calculateItemsForUrl(url);
    },
    selectedEntity(entity) {
      if (!entity) return;
      if (!this.$eth.isConnected) {
        this.step = 2;
      } else {
        this.step = 3;
      }
    },
  },
  methods: {
    calculateItemsForUrl(url) {
      if (!url) return [];
      const [isRaribleSupported, raribleERC721] = tryParseRaribleLink(url);
      if (isRaribleSupported) return [raribleERC721];
      const [isOpenSeaSupported, openSeaERC721] = tryParseOpenSeaLink(url);
      if (isOpenSeaSupported) return [openSeaERC721];
      const [isGenericSupported, genericERC721] = tryParseERC721Link(url);
      if (isGenericSupported) return [genericERC721];
      return [];
    },
    connect() {
      if (!this.$eth.isConnected) {
        this.$eth.connect();
      }
      this.step = 3;
    },
    async signVerificationMessage() {
      const web3 = this.$eth.web3;
      this.signatureTimestamp = Math.floor(new Date().getTime() / 1000);
      this.signedMessage = `I wish to use the ERC721 token ${this.selectedEntity} for the purposes of receiving elevated permissions in the discord server #${this.$route.params.guildId}. This signature is valid as of ${this.signatureTimestamp}.`;
      const accounts = await web3.eth.getAccounts();
      this.signature = await web3.eth.personal.sign(
        this.signedMessage,
        accounts[0]
      );
      this.discordCommand = `!metaverse verify ${this.signature} metaverse.diamonds|v1|use_nft|${this.selectedEntity}|${this.$route.params.guildId}|${this.signatureTimestamp}`;
      this.step = 4;
    },
    selectAllText() {
      const discordCommandText = this.$refs.discordCommandText;
      const inputElement = discordCommandText.$el.querySelector('textarea');
      inputElement.select();
    },
    copyText() {
      this.selectAllText();
      document.execCommand('copy');
    }
  },
};
</script>

<style scoped>
.full-width {
  width: 100%;
}
</style>