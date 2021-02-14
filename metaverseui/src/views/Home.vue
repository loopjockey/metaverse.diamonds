<template>
  <fragment>
    <v-dialog
      :value="true"
      width="600"
      persistent
      no-click-animation
      scrollable
    >
      <v-card
        :loading="$wait.any"
        style="position: relative"
        outlined
        id="mainCard"
      >
        <banner-card-title></banner-card-title>
        <authentication-toolbar
          :address="address"
          :user="user"
        ></authentication-toolbar>
        <v-divider></v-divider>

        <v-card-text class="pa-0">
          <fragment v-if="$route.name === 'GuildList'">
            <guild-list
              v-model="selectedGuild"
              :items="guilds"
              :loading="$wait.is('getMyDiscordGuilds')"
            ></guild-list>
          </fragment>
          <fragment v-if="$route.name === 'SelectedGuild'">
            <guild-header-list
              :guild="selectedGuild"
              :isOwner="selectedGuild"
              :loading="$wait.is('getMyDiscordGuilds')"
            >
              <v-btn
                v-if="shopUrl"
                :href="shopUrl"
                target="_blank"
                text
                outlined
                >Go to shop</v-btn
              >
            </guild-header-list>
            <guild-disconnected-state
              v-if="!serverConnected"
            ></guild-disconnected-state>
            <role-list
              v-else
              v-model="selectedRole"
              :items="roles"
              :loading="$wait.is('getGuildRewards')"
              @click="tryClaimReward($event)"
            >
            </role-list>
          </fragment>
        </v-card-text>

        <fragment v-if="$route.name === 'SelectedGuild'">
          <v-divider></v-divider>
          <v-card-actions>
            <back-button @click="$router.push('/')"></back-button>
            <v-spacer></v-spacer>
          </v-card-actions>
        </fragment>
      </v-card>
    </v-dialog>
    <wallet-check-bottom-sheet
      v-model="address"
      :user="user"
    ></wallet-check-bottom-sheet>
  </fragment>
</template>

<script>
import Api from "@/lib/Api.ts";
import { waitFor } from "vue-wait";
import BannerCardTitle from "@/components/Common/BannerCardTitle.vue";
import BackButton from "@/components/Common/BackButton.vue";
import WalletCheckBottomSheet from "@/components/Domain.Ethereum/WalletCheckBottomSheet.vue";
import AuthenticationToolbar from "@/components/Common/AuthenticationToolbar.vue";
import GuildList from "@/components/Domain.Guilds/GuildList.vue";
import GuildHeaderList from "@/components/Domain.Guilds/GuildHeaderList.vue";
import GuildDisconnectedState from "@/components/Domain.Guilds/GuildDisconnectedState.vue";
import RoleList, {
  convertRewardsToRoles,
} from "@/components/Domain.Roles/RoleList.vue";
export default {
  components: {
    BackButton,
    BannerCardTitle,
    AuthenticationToolbar,
    WalletCheckBottomSheet,
    RoleList,
    GuildList,
    GuildHeaderList,
    GuildDisconnectedState,
  },
  data() {
    return {
      isDiscordAuthenticated: Api.isDiscordAuthenticated(),
      loading: false,
      selectedRole: null,
      selectedGuild: null,
      routeGuildId: null,
      guilds: [],
      roles: [],
      serverConnected: true,
      user: null,
      shopUrl: null,
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
        const guildId = val.params.guildId;
        if (!guildId) return;
        this.getGuildRewards(guildId);
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
      this.selectedGuild = this.guilds.find(
        (g) => g.id === this.$route.params.guildId
      );
    }),
    getGuildRewards: waitFor("getGuildRewards", async function (id) {
      try {
        this.serverConnected = true;
        const rewardsResponse = await Api.create()
          .get(`guilds/${id}/rewards/mine`)
          .then((r) => r.data);
        this.roles = convertRewardsToRoles(rewardsResponse);
        this.shopUrl = rewardsResponse.shopUrl;
      } catch (err) {
        if (err.response.status === 404) this.serverConnected = false;
      }
    }),
    async tryClaimReward(reward) {
      const guildId = this.selectedGuild.id;
      this.$wait.start(`claimingReward-${reward.id}`);
      try {
        await Api.create().post(
          `guilds/${guildId}/rewards/${reward.id}/apply/${reward.availableRoleToken}`
        );
        reward.hasRole = true;
      } finally {
        this.$wait.end(`claimingReward-${reward.id}`);
      }
    },
  },
};
</script>