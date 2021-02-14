<template>
  <v-snackbar v-model="show" :color="color" :timeout="timeout">
    {{ text }}
    <v-btn dark text @click="show = false">Close</v-btn>
  </v-snackbar>
</template>

<script>
import EventBus from "@/lib/EventBus";

export function toastError(data) {
  EventBus.$emit('$toast.error', data);
}

export function toastSuccess(data) {
  EventBus.$emit('$toast.success', data);
}

export function toastInfo(data) {
  EventBus.$emit('$toast.info', data);
}

export default {
  data() {
    return {
      show: false,
      color: "",
      timeout: 5000,
      text: ""
    };
  },
  created() {
    EventBus.$on("$toast.error", this.onError);
    EventBus.$on("$toast.success", this.onSuccess);
    EventBus.$on("$toast.info", this.onInfo);
  },
  destroyed() {
    EventBus.$off("$toast.error", this.onError);
    EventBus.$off("$toast.success", this.onSuccess);
    EventBus.$off("$toast.info", this.onInfo);
  },
  methods: {
    render(text, color) {
      this.text = text;
      this.color = color;
      this.show = true;
    },
    onError(text, error) {
      this.render(text, "error");
    },
    onSuccess(text) {
      this.render(text, "success");
    },
    onInfo(text) {
      this.render(text, "info");
    }
  }
};
</script>
