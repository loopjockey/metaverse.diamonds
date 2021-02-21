<template>
  <v-skeleton-loader
    :loading="loading"
    type="list-item-avatar@6"
    :class="loading ? 'pa-2' : null"
  >
    <v-list subheader v-if="items.length">
      <v-subheader>Rewards</v-subheader>
      <fragment v-for="(role, i) in items" :key="i">
        <v-list-item>
          <v-list-item-avatar>
            <v-avatar size="42">
              <v-img
                contain
                :src="`https://ui-avatars.com/api/?background=${
                  role.colour === '000000' ? 'ff7143' : role.colour
                }&color=fff&name=${role.name}`"
              ></v-img>
            </v-avatar>
          </v-list-item-avatar>
          <v-list-item-content>
            <v-list-item-title>{{ role.name }}</v-list-item-title>
            <v-list-item-subtitle>{{
              role.isAdministrator
                ? "Administrators"
                : role.name === "@everyone"
                ? "Default"
                : "Custom"
            }}</v-list-item-subtitle>
          </v-list-item-content>
          <v-list-item-action>
            <v-btn
              color="success"
              :disabled="
                role.hasRole ||
                !role.availableRoleToken ||
                $wait.is(`claimingReward-${role.id}`)
              "
              :loading="$wait.is(`claimingReward-${role.id}`)"
              @click="$emit('click', role)"
            >
              <v-icon left>{{
                role.hasRole
                  ? "done"
                  : role.availableRoleToken
                  ? "lock_open"
                  : "lock"
              }}</v-icon>
              {{
                role.hasRole
                  ? "Active"
                  : role.availableRoleToken
                  ? "Claim Reward"
                  : "Unavailable"
              }}
            </v-btn>
          </v-list-item-action>
        </v-list-item>
        <v-divider inset v-if="i != items.length - 1"></v-divider>
      </fragment>
    </v-list>
    <no-available-roles-state v-else></no-available-roles-state>
  </v-skeleton-loader>
</template>

<script>
import NoAvailableRolesState from "@/components/Domain.Roles/NoAvailableRolesState.vue";
export function convertRewardsToRoles(rewardsResponse) {
  return rewardsResponse.allRoles.map((r) => ({
    ...r,
    availableRoleToken: rewardsResponse.applicableRoles[r.id],
    hasRole: rewardsResponse.currentRoleIds.includes(r.id),
  }));
}

export default {
  components: { NoAvailableRolesState },
  props: {
    loading: {
      default: false,
      type: Boolean,
    },
    items: {
      default: null,
      type: Array,
    },
    value: {
      default: null,
      type: Object,
    },
  },
};
</script>