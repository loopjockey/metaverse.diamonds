<template>
  <v-app class="pretty-bottom-logo">
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
::-webkit-scrollbar {
  width: 3px;
}

/* Track */
::-webkit-scrollbar-track {
  background: #f1f1f1;
}

/* Handle */
::-webkit-scrollbar-thumb {
  background: #ff7143;
}

/* Handle on hover */
::-webkit-scrollbar-thumb:hover {
  background: #555;
}

/*.pretty-bottom-logo {
  background-image: url(https://metaversediamondspublic.blob.core.windows.net/public/metaverse_diamonds_logo.png) !important;
  background-size: 400px 580px !important;
  background-repeat: no-repeat !important;
  background-position: bottom -100px right -100px !important;
}*/
</style>