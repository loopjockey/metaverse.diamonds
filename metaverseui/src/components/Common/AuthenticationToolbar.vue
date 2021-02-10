<template>
  <v-toolbar flat extended>
    <v-avatar class="mr-4" color="primary">
      <span v-if="!user" class="white--text headline">...</span>
      <v-img v-else :src="user.url"></v-img>
    </v-avatar>
    <v-toolbar-title
      >{{ user ? user.name : "loading#123..." }}
    </v-toolbar-title>
    <v-spacer></v-spacer>
    <v-menu offset-y bottom>
      <template #activator="{ on }">
        <v-btn text v-on="on">
          <v-icon left>account_balance_wallet</v-icon>
          {{ address ? address.substring(0, 10) + "..." : "connect" }}
        </v-btn>
      </template>
      <v-list>
        <v-list-item @click="logout()">
            <v-list-item-content>
                <v-list-item-title>
                    Logout
                </v-list-item-title>
            </v-list-item-content>
        </v-list-item>
      </v-list>
    </v-menu>
    <template #extension>
      <v-tabs>
        <v-tab>Guilds</v-tab>
      </v-tabs>
    </template>
  </v-toolbar>
</template>

<script>
import Api from "@/lib/Api.ts";
export default {
  props: ["address", "user"],
  methods: {
    logout() {
      localStorage.clear();
      Api.initiateDiscordLogin();
    },
  },
};
</script>