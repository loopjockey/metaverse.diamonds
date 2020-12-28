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
          <v-autocomplete
            v-model="selectedEntity"
            :search-input.sync="suppliedUrl"
            :items="items"
            :prepend-inner-icon="selectedEntity ? 'done' : 'language'"
            :disabled="!!selectedEntity"
            solo
            hide-details
            :filter="() => true"
            label="Enter Token URL"
            flat
          >
            <template #no-data>
              <v-container>
                Please enter your Token URL. Examples of token URLs are:
                <ul>
                  <li>https://app.rarible.com/token/0x...123:456</li>
                  <li>https://opensea.io/assets/0x...123/456</li>
                  <li>erc721://0x...123/456</li>
                </ul>
              </v-container>
            </template>
            <template #item>
              <v-icon color="success" left>done</v-icon>
              Valid URL! (Click to use)
            </template>
          </v-autocomplete>
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
    };
  },
  watch: {
    suppliedUrl(url) {
      this.items = this.calculateItemsForUrl(url);
    },
    selectedEntity(entity) {
      if (!entity) return;
      this.requestingSignature = true;
      try {
        //
      } catch (err) {
        //
      } finally {
        this.requestingSignature = false;
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
  },
};
</script>

<style scoped>
.full-width {
  width: 100%;
}
</style>