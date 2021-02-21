<template>
    <section class="pb-5 px-8">
        <p class="text-center mb-0" v-if="icon">
            <v-icon class="empty-state">{{icon}}</v-icon>
        </p>
        <p class="text-center">{{message}}</p>
        <p class="text-center">
            <slot></slot>
        </p>
    </section>
</template>
<script lang="ts">
export default {
    props: ['icon', 'message']
};
</script>
<style scoped>
i.empty-state {
    font-size: 60px;
    padding: 20px;
}
</style>