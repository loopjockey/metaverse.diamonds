<template>
    <v-btn @click="reloadWindow" icon :loading="loading">
        <v-icon>refresh</v-icon>
    </v-btn>
</template>

<script>
export default {
    data() {
        return { loading: false };
    },
    methods: {
        reloadWindow() {
            window.location.reload();
            this.loading = true;
        }
    }
}
</script>