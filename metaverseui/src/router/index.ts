import Vue from "vue";
import VueRouter, { RouteConfig } from "vue-router";
import Home from "@/views/Home.vue";
import Api from '@/lib/Api.ts';
import LoginCallback from "@/views/LoginCallback.vue";
import Passthrough from "@/views/Passthrough.vue";

Vue.use(VueRouter);

const guardDiscordAuthenticated = (to, from, next) => {
  if (!Api.isDiscordAuthenticated()) {
    Api.initiateDiscordLogin();
  }
  else {
    next();
  }
}

const routes: Array<RouteConfig> = [
  {
    path: "/",
    component: Home,
    beforeEnter: guardDiscordAuthenticated,
    children: [
      {
        path: "",
        component: Passthrough,
        name: "GuildList",
      },
      {
        path: ":guildId?", 
        component: Passthrough,
        beforeEnter: guardDiscordAuthenticated,
        name: "SelectedGuild"
      }
    ]
  },
  {
    path: "/login/callback",
    name: "LoginCallback",
    component: LoginCallback
  }
];

const router = new VueRouter({
  mode: "history",
  base: process.env.BASE_URL,
  routes
});

export default router;
