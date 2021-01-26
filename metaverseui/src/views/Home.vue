<template>
  <div>
    <v-dialog
      :value="true"
      width="600"
      persistent
      no-click-animation
      scrollable
      :fullscreen="$vuetify.breakpoint.xsOnly"
    >
      <v-card
        :loading="$wait.is('getMyDiscordGuilds')"
        style="position: relative"
        id="mainCard"
      >
        <v-card-title
          class="primary--text"
          style="font-family: 'Bungee', cursive"
          >metaverse.diamonds</v-card-title
        >
        <v-card-subtitle> The glue between Ξ NFTs and guilds. </v-card-subtitle>
        <v-toolbar flat extended>
          <v-avatar class="mr-4" color="primary">
            <span v-if="!user" class="white--text headline">...</span>
            <v-img v-else :src="user.url"></v-img>
          </v-avatar>
          <v-toolbar-title>{{
            user ? user.name : "loading#123..."
          }}          
          </v-toolbar-title>
          <v-spacer></v-spacer>
          <v-btn text>
            <v-icon left>account_balance_wallet</v-icon>
            {{ address ? address.substring(0, 10) + "..." : "connect" }}
          </v-btn>
          <template #extension>
            <v-tabs>
              <v-tab>Guilds</v-tab>
            </v-tabs>
          </template>
        </v-toolbar>

        <v-card-text class="pa-0">
          <fragment v-if="$route.name === 'GuildList'">
            <guild-list
              v-model="selectedGuild"
              :items="guilds"
              :loading="$wait.is('getMyDiscordGuilds')"
            ></guild-list>
          </fragment>
        </v-card-text>

        <v-divider></v-divider>
        <v-card-actions>
          <v-btn
            v-if="$route.name === 'SelectedGuild'"
            outlined
            large
            @click="$router.push('/')"
          >
            <v-icon left>keyboard_backspace</v-icon>
            Back
          </v-btn>
          <v-spacer></v-spacer>
          <v-btn
            v-if="$route.name === 'GuildList'"
            color="primary"
            outlined
            large
            :disabled="!selectedGuild"
            @click="$router.push(`/${selectedGuild ? selectedGuild.id : ''}`)"
          >
            <v-icon left>done</v-icon>
            {{
              selectedGuild
                ? `Go to ${
                    selectedGuild.name.length > 15
                      ? selectedGuild.name.substring(0, 15) + "..."
                      : selectedGuild.name
                  }`
                : "Select guild"
            }}</v-btn
          >
        </v-card-actions>
      </v-card>
    </v-dialog>
    <wallet-check-bottom-sheet v-model="address" :user="user"></wallet-check-bottom-sheet>
  </div>
</template>

<script>
import Api from "@/lib/Api.ts";
import { waitFor } from "vue-wait";
import GuildList from "@/components/Domain.Guilds/GuildList.vue";
import WalletCheckBottomSheet from "@/components/Domain.Ethereum/WalletCheckBottomSheet.vue";
export default {
  components: { GuildList, WalletCheckBottomSheet },
  data() {
    return {
      isDiscordAuthenticated: Api.isDiscordAuthenticated(),
      loading: false,
      selectedGuild: null,
      routeGuildId: null,
      guilds: [],
      rewards: [],
      user: null,
      address: Api.authData.currentAddress,
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
      deep: true,
    },
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