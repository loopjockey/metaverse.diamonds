<template>
  <v-skeleton-loader
    :loading="loading"
    type="list-item-avatar@6"
  >
    <v-list subheader>
      <v-subheader>Select Guild</v-subheader>
      <fragment v-for="(guild, i) in items" :key="i">
        <v-list-item @click="$emit('input', guild)">
          <v-list-item-avatar>
            <v-avatar>
              <v-img :src="guild.avatarUrl"></v-img>
            </v-avatar>
          </v-list-item-avatar>
          <v-list-item-content>
            <v-list-item-title>{{ guild.name }}</v-list-item-title>
            <v-list-item-subtitle>{{
              guild.isOwner ? "Owner" : "Member"
            }}</v-list-item-subtitle>
          </v-list-item-content>
          <v-list-item-action>
            <v-icon :color="value && guild.id === value.id ? 'primary' : null">
              {{
                value && guild.id === value.id
                  ? "radio_button_checked"
                  : "radio_button_unchecked"
              }}
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
    }
};
</script>