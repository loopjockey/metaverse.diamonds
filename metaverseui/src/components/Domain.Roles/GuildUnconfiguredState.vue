<template>
  <empty-state>
    This guild has not been configured yet. Please enter your
    <a target="_blank" href="https://opensea.io/collections"
      >OpenSea collection URL</a
    >
    below to generate the command for your guild. Make sure to replace
    <u>@RewardRole</u> with whichever role you would like to reward users with
    for owning your NFTs. You can enter the command below in any channel.<br /><br />
    <v-text-field
      v-model="openSeaUrl"
      rounded
      solo-inverted
      flat
      :prepend-inner-icon="openSeaCollectionId ? 'done' : 'web'"
      :rules="[
        (v) =>
          (!!v && !!openSeaCollectionId) ||
          'Your open sea collection URL should be in the form: https://opensea.io/assets/my-collection',
      ]"
      placeholder="e.g. https://opensea.io/assets/my-collection"
    >
    </v-text-field>
    <discord-messages style="text-align: left !important">
      <discord-message
        :author="user ? user.name : 'loading#123...'"
        :avatar="user ? user.url : null"
      >
        !metaverse rewards add opensea
        {{ openSeaCollectionId || "my-collection" }} @RewardRole
      </discord-message>
    </discord-messages>
  </empty-state>
</template>

<script>
import EmptyState from "@/components/Common/EmptyState.vue";
export default {
  props: ["user"],
  components: { EmptyState },
  data() {
    return { openSeaUrl: null };
  },
  computed: {
    openSeaCollectionId() {
      if (
        !this.openSeaUrl ||
        !this.openSeaUrl.trim().startsWith("https://opensea.io/assets/")
      ) {
        return null;
      }
      const parts = this.openSeaUrl.split("/").filter((p) => p);
      const collectionId = parts[parts.length - 1];
      debugger;
      return collectionId;
    },
  },
};
</script>