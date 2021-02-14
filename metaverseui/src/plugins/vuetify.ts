import "material-design-icons-iconfont/dist/material-design-icons.css";
import "roboto-fontface/css/roboto/roboto-fontface.css";
import Vue from "vue";
import Vuetify from "vuetify/lib";
import { Plugin } from 'vue-fragment';
Vue.use(Plugin)

Vue.use(Vuetify);

export default new Vuetify({
    icons: {
        iconfont: 'mdi', // default - only for display purposes
    },
    theme: {
        dark: true,
        themes: {
            dark: {
                primary: '#ff7143',
                secondary: '#eedbcf'
            },
            light: {
                primary: '#ff7143',
                secondary: '#eedbcf'
            }
        }
    }
});  
