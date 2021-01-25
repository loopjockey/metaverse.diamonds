import Vue from "vue";
import App from "./App.vue";
import "./registerServiceWorker";
import router from "./router";
import vuetify from "./plugins/vuetify";
import VueEthereum from 'vue-ethereum';
import VueWait from 'vue-wait';
import Clipboard from 'v-clipboard';
Vue.use(VueEthereum);
Vue.use(Clipboard);
Vue.use(VueWait);

Vue.config.productionTip = false;

new Vue({
  router,
  vuetify,
  render: h => h(App),
  eth: new VueEthereum(),
  wait: new VueWait()
} as any).$mount("#app");
