'use strict';

const EXTENSION_ID = 'enterpriseAiPlatform';

const SECRET_KEYS = Object.freeze({
  tokenSet: 'enterpriseAiPlatform.tokenSet'
});

const GLOBAL_STATE_KEYS = Object.freeze({
  principal: 'enterpriseAiPlatform.principal'
});

module.exports = {
  EXTENSION_ID,
  GLOBAL_STATE_KEYS,
  SECRET_KEYS
};
