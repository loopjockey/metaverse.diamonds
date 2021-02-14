<template>
  <empty-state
    icon="warning"
    message="The Metaverse Diamonds#7589 bot is not yet connected to this server. If you are the server owner please follow the link below to connect this bot to your server!"
  >
    <v-btn
      color="primary"
      target="_blank"
      outlined
      href="https://discord.com/oauth2/authorize?client_id=790559120603873320&scope=bot&permissions=268436528"
    >
      <v-icon left>auto_awesome</v-icon>
      Connect
    </v-btn>
  </empty-state>
</template>

<script>
import EmptyState from "@/components/Common/EmptyState.vue";
export default {
  components: { EmptyState },
};
</script>