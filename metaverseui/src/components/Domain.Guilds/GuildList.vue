<template>
  <v-skeleton-loader
    :loading="loading"
    type="list-item-avatar@6"
    :class="loading ? 'pa-2' :null"
  >
    <v-list subheader>
      <fragment v-for="(guild, i) in items" :key="i">
        <v-list-item @click="selectGuild(guild)">
          <v-list-item-avatar>
            <v-avatar size="42">
              <v-img :src="guild.avatarUrl || `https://ui-avatars.com/api/?background=ff7143&color=fff&name=${guild.name}`"></v-img>
            </v-avatar>
          </v-list-item-avatar>
          <v-list-item-content>
            <v-list-item-title>{{ guild.name }}</v-list-item-title>
            <v-list-item-subtitle>{{
              guild.isOwner ? "Owner" : "Member"
            }}</v-list-item-subtitle>
          </v-list-item-content>
          <v-list-item-action>
            <v-icon>
              chevron_right
            </v-icon>
          </v-list-item-action>
        </v-list-item>
        <v-divider inset v-if="i != items.length - 1"></v-divider>
      </fragment>
    </v-list>
  </v-skeleton-loader>
</template>

<script>
export default {
    props: {
        loading: {
            default: false,
            type: Boolean
        },
        items: {
            default: null,
            type: Array
        },
        value: {
            default: null,
            type: Object
        }
    },
    methods: {
      selectGuild(guild) {
        this.$emit('input', guild);
        this.$router.push(`/${guild.id}`);
      }
    }
};
</script>