<template>
    <div>applying credentials...</div>
</template>

<script>

import Api from '@/lib/Api.ts';
export default {
    created() {
        const hash = this.$route.hash;
        const params = new URLSearchParams(hash);
        Api.applyDiscordCredentials(params);
        this.$router.push('/');
    }
}
</script>