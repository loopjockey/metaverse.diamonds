<template>
  <v-app id="inspire" class="pretty-bottom-logo">
      <router-view></router-view>
  </v-app>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
  name: "App",

  data: () => ({
    //
  }),
});
</script>

<style>
.pretty-bottom-logo {
  background: url(/logo.png) !important;
  background-size: 300px 480px !important;
  background-repeat: no-repeat !important;
  background-position: bottom -100px right -100px !important;
}
</style>