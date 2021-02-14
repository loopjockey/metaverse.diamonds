<template>
  <empty-state
    icon="autorenew"
    message="You do not currently own any NFTs that convey additional rights in this server. If you think this is a mistake please contact your guild admin. Alternatively you can go to the shop nominated above to see the list of available NFTs for this server."
  >
  </empty-state>
</template>

<script>
import EmptyState from "@/components/Common/EmptyState.vue";
export default {
  components: { EmptyState },
};
</script>