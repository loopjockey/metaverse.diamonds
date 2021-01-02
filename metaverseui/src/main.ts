import Vue from "vue";
import App from "./App.vue";
import "./registerServiceWorker";
import router from "./router";
import vuetify from "./plugins/vuetify";
import VueEthereum from 'vue-ethereum'
import Clipboard from 'v-clipboard'
Vue.use(VueEthereum);
Vue.use(Clipboard)

Vue.config.productionTip = false;

new Vue({
  router,
  vuetify,
  render: h => h(App),
  eth: new VueEthereum()
}).$mount("#app");
