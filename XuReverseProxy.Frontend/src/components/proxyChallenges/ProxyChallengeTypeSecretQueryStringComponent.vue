<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
  },
})
export default class ProxyChallengeTypeSecretQueryStringComponent extends Vue {
  @Prop()
  options: any;

  // loginService: LoginService = new LoginService();

  async mounted() {}

  // get isLoading(): boolean { return this.loginService.status.inProgress; }
}
</script>

<template>
  <div class="challenge-secret-qs">
    <div class="challenge-header">
      <div class="material-icons icon">key</div>
      <div class="challenge-title">Missing secret query string value</div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.challenge-secret-qs {
  .challenge-title {
    font-weight: 600;
    color: var(--color--warning-base);
  }

  .icon {
    margin-right: 5px;
    color: var(--color--warning-base);
  }
}
</style>
