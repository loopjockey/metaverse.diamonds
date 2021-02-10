<template>
  <v-skeleton-loader
    :loading="loading"
    type="list-item-avatar"
    :class="loading ? 'pa-2' : null"
  >
    <v-list>
      <v-list-item>
        <v-list-item-avatar>
          <v-avatar class="mr-4" color="primary">
            <v-img
              :src="
                (guild || {}).avatarUrl ||
                `https://ui-avatars.com/api/?background=ff7143&color=fff&name=${
                  (guild || {}).name || '...'
                }`
              "
            ></v-img>
          </v-avatar>
        </v-list-item-avatar>
        <v-list-item-content>
          <v-list-item-title>
            {{ guild ? guild.name : "..." }}
          </v-list-item-title>
        </v-list-item-content>
        <v-list-item-action>
          <slot></slot>
        </v-list-item-action>
      </v-list-item>
    </v-list>
  </v-skeleton-loader>
</template>

<script>
export default {
  props: ["guild", "isOwner", "loading"],
};
</script>