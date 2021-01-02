import Vue from "vue";
import Vuetify from "vuetify/lib";
import "material-design-icons-iconfont/dist/material-design-icons.css";
import "roboto-fontface/css/roboto/roboto-fontface.css";
import { Plugin } from 'vue-fragment';
Vue.use(Plugin)

Vue.use(Vuetify);

export default new Vuetify({
    theme: {
        themes: {
            light: {
                primary: '#d2927d',
                secondary: '#eedbcf'
            }
        }
    }
});  