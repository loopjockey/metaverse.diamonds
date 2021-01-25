<template>
  <v-dialog
    :value="true"
    width="600"
    persistent
    retain-focus
    hide-overlay
    no-click-animation
    scrollable
  >
    <v-card :loading="$wait.is('getMyDiscordGuilds')" class="mx-auto">
      <v-toolbar flat extended>
        <v-avatar class="mr-3">
          <v-img :src="user.url"></v-img>
        </v-avatar>
        <v-toolbar-title>{{ user.name }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn @click="logout" text>Logout</v-btn>
        <template #extension>
          <v-tabs>
            <v-tab>Guilds</v-tab>
          </v-tabs>
        </template>
      </v-toolbar>

      <v-card-text class="pa-0">
        <guild-list
          v-model="selectedGuild"
          :items="guilds"
          :loading="$wait.is('getMyDiscordGuilds')"
        ></guild-list>
      </v-card-text>

      <v-divider></v-divider>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn
          color="primary"
          outlined
          large
          :disabled="!selectedGuild"
          @click="$router.push(`/${selectedGuild ? selectedGuild.id : ''}`)"
        >
          <v-icon left>done</v-icon>
          Next</v-btn
        >
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
import Api from "@/lib/Api.ts";
import { waitFor } from "vue-wait";
import GuildList from "@/components/Domain.Guilds/GuildList.vue";
export default {
  components: { GuildList },
  data() {
    return {
      isDiscordAuthenticated: Api.isDiscordAuthenticated(),
      loading: false,
      selectedGuild: null,
      routeGuildId: null,
      guilds: [],
      rewards: [],
      user: {},
    };
  },
  created() {
    if (!Api.isDiscordAuthenticated()) return;
    this.getMyDiscordGuilds();
  },
  watch: {
    $route: {
      handler(val) {
        if (!val) return;
      },
      immediate: true,
      deep: true
    }
  },
  methods: {
    getMyDiscordGuilds: waitFor("getMyDiscordGuilds", async function () {
      const guildsResponse = await Api.create()
        .get("guilds")
        .then((r) => r.data);
      this.guilds = guildsResponse.guilds.map((g) => ({
        ...g,
        selected: false,
      }));
      this.user = guildsResponse.user;
    }),
    async startNextStep() {
      /*if (!this.$eth.isConnected) {
        this.$eth.connect();
      }
      await this.getEthereumSignature();
      await this.getGuildRewards(this.selectedGuildId);*/
    },
    getEthereumSignature: waitFor("getEthereumSignature", async function () {
      const web3 = this.$eth.web3;
      const accounts = await web3.eth.getAccounts();
      const defaultAccount = accounts[0];
      const user = this.user.name;
      const expiry = Api.authData.expiryTime?.toDate().getTime();
      const message = `I agree to link this user ${user} to my current address. Expires: ${expiry}`;
      const signedMessage = await web3.eth.personal.sign(
        message,
        defaultAccount
      );
      Api.applyEthereumCredentials(signedMessage);
    }),
    getGuildRewards: waitFor("getGuildRewards", async function (id) {
      this.rewards = await Api.create()
        .get(`guilds/${id}/rewards`)
        .then((r) => r.data);
    }),
    logout() {
      localStorage.clear();
      Api.initiateDiscordLogin();
    },
  },
};
</script>