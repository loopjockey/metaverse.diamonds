<template>
  <v-app class="pretty-bottom-logo">
    <router-view></router-view>
    <toast></toast>
  </v-app>
  <!--<footer
      style="
        position: fixed;
        bottom: 0;
        width: 100%;
        color: white;
        z-index: 100000;
      "
    >
      TODO!!!!!!
    </footer>-->
</template>

<script lang="ts">
import Vue from "vue";
import Toast from "@/components/Common/Toast.vue";
export default Vue.extend({
  name: "App",
  components: { Toast },
  data: () => ({
    sheet: true,
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

.pretty-bottom-logo::before {
  content: "";
  background-image: url(https://metaversediamondspublic.blob.core.windows.net/public/background_2.jpg) !important;
  background-size: cover;
  background-position: center;
  position: absolute;
  top: 0px;
  right: 0px;
  bottom: 0px;
  left: 0px;
  opacity: 0.3;
}
</style>